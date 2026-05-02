import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { readdirSync } from 'fs';
import path from 'path';
import { queryDb } from '../db.js';

export function registerMigrationStatus(server: McpServer) {
  server.tool(
    'migration_status',
    'Check which EF Core migrations are applied vs pending.',
    {},
    async () => {
      const rows = await queryDb('SELECT MigrationId FROM __EFMigrationsHistory') as any[];
      const applied: string[] = rows.map((r) => r.MigrationId).sort();

      const dir = process.env.MIGRATIONS_DIR!;
      const onDisk = readdirSync(dir)
        .filter((f) => f.endsWith('.cs') && !f.endsWith('.Designer.cs') && !f.includes('Snapshot'))
        .map((f) => path.basename(f, '.cs'))
        .sort();

      const appliedSet = new Set(applied);
      const pending = onDisk.filter((m) => !appliedSet.has(m));

      return {
        content: [{
          type: 'text',
          text: JSON.stringify({
            applied,
            pending,
            lastApplied: applied.at(-1) ?? null,
            totalApplied: applied.length,
            totalPending: pending.length,
          }, null, 2),
        }],
      };
    },
  );
}
