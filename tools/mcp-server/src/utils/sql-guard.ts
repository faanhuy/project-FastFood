const FORBIDDEN = /\b(INSERT|UPDATE|DELETE|DROP|TRUNCATE|ALTER|CREATE|EXEC(UTE)?|sp_\w+|xp_\w+)\b/i;
const STRIP_COMMENTS = /(--[^\n]*|\/\*[\s\S]*?\*\/)/g;

export function assertSelectOnly(sql: string): void {
  const clean = sql.replace(STRIP_COMMENTS, '').trim();
  if (!/^SELECT\b/i.test(clean)) throw new Error('Only SELECT queries are allowed.');
  if (FORBIDDEN.test(clean)) throw new Error('Query contains forbidden keyword.');
}
