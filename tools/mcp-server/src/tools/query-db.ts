import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { z } from 'zod';
import { queryDb } from '../db.js';
import { assertSelectOnly } from '../utils/sql-guard.js';

export function registerQueryDb(server: McpServer) {
  server.tool(
    'query_db',
    'Execute a read-only SELECT query against the SmartShop SQL Server database.',
    {
      sql: z.string().describe('SELECT query to execute'),
      limit: z.number().min(1).max(200).default(50).describe('Max rows to return (default 50, max 200)'),
    },
    async ({ sql, limit }) => {
      assertSelectOnly(sql);
      const limitedSql = `SELECT TOP(${limit}) * FROM (${sql}) AS _mcp_q`;
      const rows = await queryDb(limitedSql);
      return {
        content: [{ type: 'text', text: JSON.stringify(rows, null, 2) }],
      };
    },
  );
}
