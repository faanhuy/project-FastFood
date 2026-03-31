const fs = require("node:fs");
const path = require("node:path");
const { marked } = require("marked");
const { chromium } = require("playwright");

function slugify(s) {
  return String(s || "")
    .trim()
    .toLowerCase()
    .replace(/[`"'“”‘’]/g, "")
    .replace(/[^a-z0-9\u00C0-\u024F\u1E00-\u1EFF]+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "");
}

function extractTitle(md) {
  const m = md.match(/^#\s+(.+)\s*$/m);
  return m ? m[1].trim() : "Tài liệu (bản dịch)";
}

function extractMetaLines(md) {
  const lines = md.split(/\r?\n/).slice(0, 12);
  const meta = lines
    .filter((l) => /tác giả|bản quyền|author|copyright/i.test(l))
    .map((l) => l.replace(/\s+$/, "").replace(/<br\s*\/?>/gi, ""))
    .join("  \n");
  return meta;
}

function addHeadingAnchors(html) {
  // Add stable IDs so a TOC can link to headings.
  return html.replace(
    /<(h2|h3|h4)>([\s\S]*?)<\/\1>/g,
    (full, tag, inner) => {
      const text = inner.replace(/<[^>]+>/g, "").trim();
      const id = slugify(text);
      if (!id) return full;
      return `<${tag} id="${id}">${inner}</${tag}>`;
    }
  );
}

function buildTocFromMarkdown(md) {
  const lines = md.split(/\r?\n/);
  const items = [];
  for (const line of lines) {
    const m = line.match(/^(#{2,4})\s+(.+?)\s*$/);
    if (!m) continue;
    const level = m[1].length; // 2..4
    const text = m[2].trim();
    if (!text) continue;
    items.push({ level, text, id: slugify(text) });
  }
  // Keep it short: only h2/h3 for TOC.
  const filtered = items.filter((i) => i.level <= 3);
  let html = `<section class="toc">\n<h2>Mục lục</h2>\n<ul>\n`;
  for (const it of filtered) {
    const indent = it.level === 3 ? ' style="margin-left: 16px"' : "";
    html += `<li${indent}><a href="#${it.id}">${it.text}</a></li>\n`;
  }
  html += `</ul>\n</section>\n`;
  return html;
}

async function main() {
  const repoRoot = path.resolve(__dirname, "..");
  const inputMd = path.join(
    repoRoot,
    "docs",
    "20250423-EB-Event-Driven_Design_for_Agents_vi.md"
  );
  const cssPath = path.join(repoRoot, "scripts", "ebook.css");
  const outputPdf = path.join(
    repoRoot,
    "docs",
    "20250423-EB-Event-Driven_Design_for_Agents_vi.pdf"
  );

  if (!fs.existsSync(inputMd)) {
    throw new Error(`Không tìm thấy file: ${inputMd}`);
  }
  const md = fs.readFileSync(inputMd, "utf8");
  const title = extractTitle(md);
  const meta = extractMetaLines(md);

  marked.setOptions({
    gfm: true,
    breaks: false,
    headerIds: false, // we add our own
    mangle: false,
  });

  let bodyHtml = marked.parse(md);
  bodyHtml = addHeadingAnchors(bodyHtml);
  const tocHtml = buildTocFromMarkdown(md);
  const css = fs.readFileSync(cssPath, "utf8");

  const coverHtml = `
    <section class="cover">
      <h1>${title}</h1>
      <div class="meta">
        ${meta ? meta.replace(/\r?\n/g, "<br/>") : ""}
      </div>
      <div class="rule"></div>
      <div class="subtitle">
        Bản dịch tiếng Việt (dạng ebook PDF), định dạng tối ưu cho in ấn (A4).
      </div>
    </section>
  `;

  const fullHtml = `<!doctype html>
  <html lang="vi">
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <title>${title}</title>
      <style>${css}</style>
    </head>
    <body>
      <main>
        ${coverHtml}
        ${tocHtml}
        ${bodyHtml}
      </main>
    </body>
  </html>`;

  const browser = await chromium.launch();
  const page = await browser.newPage();
  await page.setContent(fullHtml, { waitUntil: "networkidle" });

  // Ensure layout is fully computed before PDF.
  await page.evaluate(() => document.fonts && document.fonts.ready);

  await page.pdf({
    path: outputPdf,
    format: "A4",
    printBackground: true,
    displayHeaderFooter: true,
    headerTemplate: `
      <div style="font-size:9px; width:100%; padding:0 18mm; color:#666;">
        <span>${title}</span>
      </div>`,
    footerTemplate: `
      <div style="font-size:9px; width:100%; padding:0 18mm; color:#666; display:flex; justify-content:space-between;">
        <span></span>
        <span>Trang <span class="pageNumber"></span> / <span class="totalPages"></span></span>
      </div>`,
    margin: { top: "16mm", bottom: "18mm", left: "18mm", right: "18mm" },
  });

  await browser.close();
  process.stdout.write(`OK: ${outputPdf}\n`);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});

