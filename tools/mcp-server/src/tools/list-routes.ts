import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { z } from 'zod';
import { readdirSync, readFileSync } from 'fs';
import path from 'path';

interface RouteInfo {
  method: string;
  path: string;
  auth: string;
  summary: string;
}

function parseController(filePath: string): RouteInfo[] {
  const content = readFileSync(filePath, 'utf-8');
  const routes: RouteInfo[] = [];

  const baseMatch = content.match(/\[Route\("([^"]+)"\)\]/);
  const basePath = baseMatch ? baseMatch[1] : '';

  const actionPattern =
    /(?:\/\/\/ <summary>\s*\/\/\/ ([^\n]+)\s*\/\/\/ <\/summary>[\s\S]*?)?(\[Authorize[^\]]*\]|\[AllowAnonymous\])?\s*(?:\[Authorize[^\]]*\]|\[AllowAnonymous\])?\s*\[Http(Get|Post|Put|Patch|Delete)(?:\("([^"]*)"\))?\]/g;

  let match: RegExpExecArray | null;
  while ((match = actionPattern.exec(content)) !== null) {
    const summary = match[1]?.trim() ?? '';
    const method = match[3].toUpperCase();
    const sub = match[4] ?? '';

    const fullPath = ['/' + basePath, sub].filter(Boolean).join('/').replace(/\/+/g, '/');

    const contextBefore = content.slice(Math.max(0, match.index - 200), match.index);
    let auth = 'public';
    if (/\[AllowAnonymous\]/.test(contextBefore)) auth = 'public';
    else if (/\[Authorize\(Roles\s*=\s*"Admin"\)\]/.test(contextBefore)) auth = 'admin';
    else if (/\[Authorize\]/.test(contextBefore)) auth = 'authenticated';

    routes.push({ method, path: fullPath, auth, summary });
  }

  return routes;
}

export function registerListRoutes(server: McpServer) {
  server.tool(
    'list_routes',
    'List all API routes in SmartShop WebAPI with method, path, auth requirement, and summary.',
    {
      filter: z
        .string()
        .optional()
        .describe("Optional substring filter on path or method, e.g. 'orders' or 'POST'"),
    },
    async ({ filter }) => {
      const dir = process.env.CONTROLLERS_DIR!;
      const files = readdirSync(dir).filter((f) => f.endsWith('Controller.cs'));

      const allRoutes: RouteInfo[] = [];
      for (const file of files) {
        allRoutes.push(...parseController(path.join(dir, file)));
      }

      allRoutes.sort((a, b) => a.path.localeCompare(b.path));

      const filtered = filter
        ? allRoutes.filter(
            (r) =>
              r.path.toLowerCase().includes(filter.toLowerCase()) ||
              r.method.toLowerCase().includes(filter.toLowerCase()),
          )
        : allRoutes;

      return { content: [{ type: 'text', text: JSON.stringify(filtered, null, 2) }] };
    },
  );
}
