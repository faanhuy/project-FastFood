import { spawn } from 'child_process';

const server = process.env.DB_SERVER ?? 'localhost';
const database = process.env.DB_NAME ?? 'SmartShop';

export function queryDb(sql: string): Promise<unknown[]> {
  return new Promise((resolve, reject) => {
    const escaped = sql.replace(/'/g, "''").replace(/"/g, '`"');
    const psScript = `
      [Console]::OutputEncoding = [System.Text.Encoding]::UTF8;
      $rows = Invoke-Sqlcmd -ServerInstance "${server}" -Database "${database}" -Query "${escaped}" -ErrorAction Stop;
      if ($rows) {
        $cols = $rows[0].Table.Columns | ForEach-Object { $_.ColumnName };
        $rows | ForEach-Object { $r = $_; $h = @{}; foreach ($c in $cols) { $h[$c] = $r[$c] }; [PSCustomObject]$h } | ConvertTo-Json -Depth 2 -Compress
      }
      else { "[]" }
    `.trim();

    const chunks: Buffer[] = [];
    const errChunks: Buffer[] = [];

    const proc = spawn('powershell', ['-NoProfile', '-NonInteractive', '-Command', psScript], {
      windowsHide: true,
    });

    proc.stdout.on('data', (chunk: Buffer) => chunks.push(chunk));
    proc.stderr.on('data', (chunk: Buffer) => errChunks.push(chunk));

    proc.on('close', () => {
      const stderr = Buffer.concat(errChunks).toString('utf8').trim();
      const raw = Buffer.concat(chunks).toString('utf8').trim();

      if (!raw || raw === '[]') return resolve([]);

      try {
        const parsed = JSON.parse(raw);
        resolve(Array.isArray(parsed) ? parsed : [parsed]);
      } catch {
        if (stderr) reject(new Error(stderr.slice(0, 300)));
        else reject(new Error(`Failed to parse output: ${raw.slice(0, 300)}`));
      }
    });

    proc.on('error', reject);
  });
}
