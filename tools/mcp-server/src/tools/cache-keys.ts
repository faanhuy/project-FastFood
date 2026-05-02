import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { z } from 'zod';
import { getRedis } from '../redis.js';

export function registerCacheKeys(server: McpServer) {
  server.tool(
    'cache_keys',
    'List Redis cache keys with their TTL and type. Uses SCAN (non-blocking).',
    {
      pattern: z
        .string()
        .default('*')
        .describe("Redis SCAN pattern. Default '*'. Example: 'products:*'"),
    },
    async ({ pattern }) => {
      const redis = getRedis();
      await redis.connect().catch(() => {});

      const keys: string[] = [];
      let cursor = '0';
      do {
        const [nextCursor, batch] = await redis.scan(cursor, 'MATCH', pattern, 'COUNT', 100);
        cursor = nextCursor;
        keys.push(...batch);
        if (keys.length >= 100) break;
      } while (cursor !== '0');

      const results = await Promise.all(
        keys.slice(0, 100).map(async (key) => {
          const [type, ttl] = await Promise.all([redis.type(key), redis.ttl(key)]);
          return { key, type, ttlSeconds: ttl };
        }),
      );

      results.sort((a, b) => a.key.localeCompare(b.key));

      return {
        content: [{
          type: 'text',
          text: JSON.stringify({ total: results.length, keys: results }, null, 2),
        }],
      };
    },
  );
}
