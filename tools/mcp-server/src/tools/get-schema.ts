import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { z } from 'zod';
import { queryDb } from '../db.js';
import { ALLOWED_TABLES, isSensitiveColumn } from '../utils/allowed-tables.js';

export function registerGetSchema(server: McpServer) {
  server.tool(
    'get_schema',
    'Return column definitions and constraints for SmartShop tables.',
    {
      tables: z
        .array(z.string())
        .optional()
        .describe('Table names to describe. Omit to return all allowed tables.'),
    },
    async ({ tables }) => {
      const targets = tables?.length
        ? tables.filter((t) => ALLOWED_TABLES.has(t))
        : [...ALLOWED_TABLES];

      if (!targets.length) {
        return { content: [{ type: 'text', text: 'No matching allowed tables found.' }] };
      }

      const inClause = targets.map((t) => `'${t}'`).join(',');

      const cols = await queryDb(`
        SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME IN (${inClause})
        ORDER BY TABLE_NAME, ORDINAL_POSITION
      `) as any[];

      const pks = await queryDb(`
        SELECT ku.TABLE_NAME, ku.COLUMN_NAME
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
        WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND ku.TABLE_NAME IN (${inClause})
      `) as any[];

      const pkSet = new Set(pks.map((r) => `${r.TABLE_NAME}.${r.COLUMN_NAME}`));

      const schema: Record<string, any[]> = {};
      for (const row of cols) {
        if (!schema[row.TABLE_NAME]) schema[row.TABLE_NAME] = [];
        const sensitive = isSensitiveColumn(row.COLUMN_NAME);
        schema[row.TABLE_NAME].push({
          column: row.COLUMN_NAME,
          type: sensitive ? '[REDACTED]' : row.DATA_TYPE + (row.CHARACTER_MAXIMUM_LENGTH ? `(${row.CHARACTER_MAXIMUM_LENGTH})` : ''),
          nullable: row.IS_NULLABLE === 'YES',
          isPK: pkSet.has(`${row.TABLE_NAME}.${row.COLUMN_NAME}`),
        });
      }

      return { content: [{ type: 'text', text: JSON.stringify(schema, null, 2) }] };
    },
  );
}
