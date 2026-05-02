import 'dotenv/config';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { registerQueryDb } from './tools/query-db.js';
import { registerGetSchema } from './tools/get-schema.js';
import { registerMigrationStatus } from './tools/migration-status.js';
import { registerListRoutes } from './tools/list-routes.js';
import { registerCacheKeys } from './tools/cache-keys.js';

const server = new McpServer({ name: 'smartshop', version: '1.0.0' });

registerQueryDb(server);
registerGetSchema(server);
registerMigrationStatus(server);
registerListRoutes(server);
registerCacheKeys(server);

const transport = new StdioServerTransport();
await server.connect(transport);
