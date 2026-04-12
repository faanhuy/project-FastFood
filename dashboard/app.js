// ═══════════════════════════════════════════════════════
// SmartShop Learning Tracker — app.js
// ═══════════════════════════════════════════════════════

// ── DEFAULT DATA ──────────────────────────────────────
const DEFAULT_DATA = {
  profile: {
    name: 'Phan Duy Huy',
    project: 'SmartShop',
    role: '.NET Developer · 3.5 năm kinh nghiệm',
    startDate: '2026-03-26',
    targetDate: '2026-06-30',
  },
  sprints: [
    {
      id: 'sp0', name: 'Sprint 0 — Foundational Setup',
      status: 'done', date: '26/03/2026', color: '#22c55e',
      tasks: [
        { id: 'sp0-t1', name: 'Cài đặt môi trường (.NET 8, Node.js, Docker Desktop)',     status: 'done',    notes: '' },
        { id: 'sp0-t2', name: 'Tạo cấu trúc solution 4-layer Clean Architecture',          status: 'done',    notes: '' },
        { id: 'sp0-t3', name: 'Docker Compose: SQL Server 2022 + Redis 7',                  status: 'done',    notes: '' },
        { id: 'sp0-t4', name: 'Frontend scaffold: React + Vite + TypeScript + TailwindCSS', status: 'done',    notes: '' },
        { id: 'sp0-t5', name: 'CQRS với MediatR — GetProductById query & handler',          status: 'done',    notes: '' },
        { id: 'sp0-t6', name: 'EF Core DbContext + ProductRepository + IProductRepository', status: 'done',    notes: '' },
        { id: 'sp0-t7', name: 'Initial Migration (20260326141016_InitialCreate) + DB tạo',  status: 'done',    notes: '' },
      ],
    },
    {
      id: 'sp1', name: 'Sprint 1 — Auth & Domain Entities',
      status: 'done', date: '30/03/2026', color: '#6c63ff',
      tasks: [
        { id: 'sp1-t1', name: 'Domain Entities: User, Product, Order, Cart, Category, Review + enums', status: 'done', notes: 'Tất cả 8 entities. OrderStatus enum 7 trạng thái. Đủ navigation + business methods.' },
        { id: 'sp1-t2', name: 'EF Core Migrations: InitialCreate → AddUserEntity → AddRefreshTokenToUser → AddFullSchema', status: 'done', notes: '8 IEntityTypeConfiguration files. Seed data Admin được bổ sung ở Sprint 6.' },
        { id: 'sp1-t3', name: 'IUserRepository + UserRepository (GetByRefreshTokenAsync)',  status: 'done', notes: '' },
        { id: 'sp1-t4', name: 'RegisterCommand + Handler + FluentValidation + BCrypt hash', status: 'done', notes: '' },
        { id: 'sp1-t5', name: 'LoginCommand + JWT AccessToken (15m) + RefreshToken rotation', status: 'done', notes: '' },
        { id: 'sp1-t6', name: 'AuthController: POST register / login / refresh / revoke',   status: 'done', notes: '' },
        { id: 'sp1-t7', name: 'Global Exception Handler + ApiResponse<T> wrapper',          status: 'done', notes: 'ExceptionHandlingMiddleware xử lý 400/401/409/500.' },
        { id: 'sp1-t8', name: 'React: LoginPage + RegisterPage + authStore Zustand + axios interceptor', status: 'done', notes: 'Zustand persist token. Axios interceptor tự động refresh khi 401.' },
      ],
    },
    {
      id: 'sp2', name: 'Sprint 2 — Product Catalog',
      status: 'done', date: '04/04/2026', color: '#a78bfa',
      tasks: [
        { id: 'sp2-t1', name: 'Category entity + CRUD API (GET/POST/PUT/DELETE)',             status: 'done', notes: '' },
        { id: 'sp2-t2', name: 'Product CRUD Admin + Slug auto-generation + soft delete',      status: 'done', notes: 'Slug tự sinh từ name. Soft delete dùng IsDeleted flag.' },
        { id: 'sp2-t3', name: 'Product listing (paginated, filter by category, search name)', status: 'done', notes: 'PagedResult<T> với Skip/Take. Filter kết hợp.' },
        { id: 'sp2-t4', name: 'Redis caching: product:{id} 30m, products:category:{id} 10m', status: 'done', notes: 'StackExchange.Redis. Invalidate cache khi Admin update/delete.' },
        { id: 'sp2-t5', name: 'React: ProductListPage (filter + pagination)',                 status: 'done', notes: '' },
        { id: 'sp2-t6', name: 'React: ProductDetailPage (slug URL, specs, add to cart)',      status: 'done', notes: '' },
      ],
    },
    {
      id: 'sp3', name: 'Sprint 3 — Cart & Orders',
      status: 'done', date: '07/04/2026', color: '#00d4aa',
      tasks: [
        { id: 'sp3-t1', name: 'Cart CRUD API + AddToCart / UpdateQuantity / RemoveItem',     status: 'done', notes: 'Cart.AddItem(productId, quantity, unitPrice). 1 cart/user.' },
        { id: 'sp3-t2', name: 'PlaceOrder command + OrderItem + tính tổng tiền',             status: 'done', notes: 'GetByIdAsync gọi 2 lần/item trong handler.' },
        { id: 'sp3-t3', name: 'Order status flow: Pending→Confirmed→Shipping→Delivered',     status: 'done', notes: 'resolveOrderStatus helper. Cancel chỉ được khi Pending/Confirmed.' },
        { id: 'sp3-t4', name: 'React: CartPage + CheckoutPage + order summary',              status: 'done', notes: '' },
        { id: 'sp3-t5', name: 'React: OrderHistoryPage + OrderDetailPage + cancel order',    status: 'done', notes: '' },
      ],
    },
    {
      id: 'sp4', name: 'Sprint 4 — AI Features',
      status: 'done', date: '09/04/2026', color: '#f59e0b',
      tasks: [
        { id: 'sp4-t1', name: 'Groq integration: llama-3.3-70b (chat) + llama-3.1-8b (search)', status: 'done', notes: 'Dual-model. MaxCandidates = 300 (không dùng int.MaxValue).' },
        { id: 'sp4-t2', name: 'Voyage AI embeddings (voyage-3, 1024 dims) + ProductEmbedding entity', status: 'done', notes: 'Vector similarity search. Embed khi tạo/update product.' },
        { id: 'sp4-t3', name: 'AI Semantic Search — GET /api/ai/search?q=',                 status: 'done', notes: '' },
        { id: 'sp4-t4', name: 'AI Recommendations — GET /api/ai/recommendations/{productId}', status: 'done', notes: '' },
        { id: 'sp4-t5', name: 'AI Description Generator — POST /api/ai/generate-description', status: 'done', notes: 'Admin only. Groq sinh mô tả tự động từ tên + category.' },
        { id: 'sp4-t6', name: 'React: AI Search bar tích hợp vào Navbar',                   status: 'done', notes: '' },
      ],
    },
    {
      id: 'sp5', name: 'Sprint 5 — Tests, Docker & CI/CD',
      status: 'done', date: '10/04/2026', color: '#0ea5e9',
      tasks: [
        { id: 'sp5-t1', name: '59 unit tests — xUnit + Moq + FluentAssertions (0 failures)', status: 'done', notes: 'Cover Auth, Cart, Order, Product, Review handlers.' },
        { id: 'sp5-t2', name: 'Dockerfile multi-stage cho Backend + Frontend',               status: 'done', notes: '' },
        { id: 'sp5-t3', name: 'Docker Compose full-stack (api + frontend + db + redis)',     status: 'done', notes: '' },
        { id: 'sp5-t4', name: 'GitHub Actions CI: build + test trên push master',           status: 'done', notes: '' },
        { id: 'sp5-t5', name: 'GitHub Actions CD: build & push images lên GHCR',           status: 'done', notes: 'Live tại ghcr.io/faanhui/' },
        { id: 'sp5-t6', name: 'README portfolio: badges CI, tech stack, hướng dẫn chạy',   status: 'done', notes: '' },
      ],
    },
    {
      id: 'sp6', name: 'Sprint 6 — Security, Reviews & Admin',
      status: 'done', date: '11/04/2026', color: '#ef4444',
      tasks: [
        { id: 'sp6-t1', name: 'Role-Based Auth: [Authorize(Roles="Admin")] trên ProductsController', status: 'done', notes: 'Admin seeded: admin@smartshop.vn / Admin@123' },
        { id: 'sp6-t2', name: 'Reviews API: GET/POST/DELETE — full CQRS',                   status: 'done', notes: '' },
        { id: 'sp6-t3', name: 'Order Admin: GET all + PATCH status (Admin only)',            status: 'done', notes: '' },
        { id: 'sp6-t4', name: 'User Profile: GET/PUT /api/users/me',                        status: 'done', notes: '' },
        { id: 'sp6-t5', name: 'Frontend: Navbar component (tự fetch cart/order counts)',    status: 'done', notes: '' },
        { id: 'sp6-t6', name: 'Frontend: react-hot-toast thay thế toàn bộ alert()',        status: 'done', notes: '' },
        { id: 'sp6-t7', name: 'Frontend: AdminOrderPage /admin/orders + status dropdown',   status: 'done', notes: '' },
      ],
    },
    {
      id: 'sp7', name: 'Sprint 7 — Coupon & Discount System',
      status: 'active', date: '', color: '#f97316',
      tasks: [
        { id: 'sp7-t1',  name: 'Enum DiscountType (Percentage / FixedAmount)',                       status: 'done', notes: 'Đặt tại Domain/Enums. EF Core lưu dạng int. Tránh dùng string vì dễ typo.' },
        { id: 'sp7-t2',  name: 'Entity Coupon: Create(), CalculateDiscount(), Use(), backing field', status: 'done', notes: 'Kế thừa BaseAuditableEntity. Code là unique key. MinOrderValue check trước khi tính. _usages backing field + public IReadOnlyCollection. DateTime.UtcNow nhất quán.' },
        { id: 'sp7-t3',  name: 'Entity CouponUsage: Create() + FK UserId/OrderId/CouponId',         status: 'done', notes: 'Bản ghi đơn — không có collection. Navigation: User?, Coupon?, Order?. Coupon là aggregate root chứa _usages.' },
        { id: 'sp7-t4',  name: 'EF Core Config: unique index Code, composite (CouponId, UserId)',    status: 'done', notes: 'CouponConfiguration: unique index trên Code, precision(18,2) cho decimal, cấu hình backing field _usages. CouponUsageConfiguration: composite unique (CouponId, UserId) — enforce 1 lần/user. Delete behavior: Restrict trên cả 3 FK.' },
        { id: 'sp7-t5',  name: 'AppDbContext: thêm DbSet Coupons + CouponUsages',                   status: 'done', notes: 'Thêm 2 dòng DbSet vào AppDbContext.cs.' },
        { id: 'sp7-t6',  name: 'ICouponRepository + CouponRepository + DI registration',            status: 'done', notes: 'Methods: GetByCodeAsync, GetAllAsync, AddAsync, DeleteAsync, HasUsageByUserAsync, HasAnyUsageAsync. Đăng ký Scoped trong DependencyInjection.cs.' },
        { id: 'sp7-t7',  name: 'Migration: AddCouponSystem',                                        status: 'done', notes: 'File đã rename từ AddCouponSchema → AddCouponSystem để class name khớp file name. Bài học: EF quan tâm class name trong partial class, không phải file name.' },
        { id: 'sp7-t8',  name: 'DTOs: CouponResponse, ValidateCouponResponse',                      status: 'done', notes: 'ValidateCouponResponse tách rõ 3 giá trị: OriginalAmount / DiscountAmount / FinalAmount. Tránh tên mơ hồ DiscountedAmount.' },
        { id: 'sp7-t9',  name: 'CreateCouponCommand + DeleteCouponCommand (Admin)',                  status: 'done', notes: 'Bug đã fix: handler dùng nhầm CreateCouponRequest (plain DTO) thay vì CreateCouponCommand (IRequest). Xoá file thừa CreateCouponRequest.cs.' },
        { id: 'sp7-t10', name: 'GetCouponsQuery + ValidateCouponQuery',                             status: 'done', notes: 'Validate check theo thứ tự: tồn tại → hết hạn → hết lượt → min order value → user đã dùng. Bug fix: "not found" phải là NotFoundException, không phải ConflictException.' },
        { id: 'sp7-t11', name: 'Tích hợp CouponCode vào PlaceOrderCommand',                         status: 'todo', notes: 'Thêm string? CouponCode vào command. Nếu có giá trị: validate → coupon.Use() → tạo CouponUsage → trừ discountAmount. Toàn bộ trong 1 transaction với IUnitOfWork.' },
        { id: 'sp7-t12', name: 'CouponsController: 4 endpoints',                                    status: 'done', notes: 'POST /api/coupons (Admin), DELETE /api/coupons/{code} (Admin), GET /api/coupons (Admin), POST /api/coupons/validate (User). Validate dùng POST body thay GET query — type-safe hơn với decimal.' },
        { id: 'sp7-t13', name: 'Công cụ: scaffold-feature.ps1 tự động sinh CQRS boilerplate',       status: 'done', notes: 'tools/scaffold-feature.ps1 -EntityName <Name>. Sinh 7 file: Command/Handler, Delete/Handler, Query/Handler, Controller với namespace đúng + TODO comment. File tồn tại bị SKIP — không ghi đè.' },
        { id: 'sp7-t14', name: 'Frontend: CouponInput component + tích hợp CheckoutPage',           status: 'todo', notes: 'Nhận orderTotal + callback onApply(discount). Gọi validate API khi bấm nút (không gọi onChange). Hiển thị breakdown: giá gốc → giảm → tổng. Cho phép xoá coupon đã áp dụng.' },
        { id: 'sp7-t15', name: 'Frontend: AdminCouponsPage /admin/coupons',                         status: 'todo', notes: 'Bảng: Code, Loại, Giá trị, MinOrderValue, Hết hạn, Đã dùng/Tổng, Trạng thái. Form tạo mới với validate client-side. Confirmation dialog trước khi xoá.' },
        { id: 'sp7-t16', name: 'Unit Tests: CouponTests (domain) + handler tests',                  status: 'todo', notes: 'Domain: CalculateDiscount Percentage/Fixed, throw khi MinOrderValue, throw khi expired/hết lượt, tăng UsedQuantity. Handler: ConflictException khi code trùng, khi user đã dùng, khi hết hạn.' },
      ],
    },
  ],
  skills: [
    { id: 'sk1',  name: 'C# / .NET Core',         icon: '⚙️', level: 4, category: 'Backend',        status: 'strong',   desc: '3.5 năm tại HQSOFT. Nền tảng vững để viết Clean Architecture.', notes: '' },
    { id: 'sk2',  name: 'SQL Server',              icon: '🗄️', level: 5, category: 'Database',       status: 'strong',   desc: 'Tối ưu query, execution plan, stored procedures — điểm mạnh nổi bật.', notes: '' },
    { id: 'sk3',  name: 'ASP.NET Web API',         icon: '🌐', level: 4, category: 'Backend',        status: 'strong',   desc: 'RESTful API design. Đã làm API đồng bộ dữ liệu đa hệ thống.', notes: '' },
    { id: 'sk4',  name: 'Clean Architecture',      icon: '🏛️', level: 3, category: 'Architecture',   status: 'learning', desc: 'Đang áp dụng trong SmartShop. CQRS + Repository đang chạy tốt.', notes: '' },
    { id: 'sk5',  name: 'Entity Framework Core',   icon: '📦', level: 3, category: 'Backend',        status: 'learning', desc: 'Migrations, Fluent API, DbContext config. Cần thực hành thêm.', notes: '' },
    { id: 'sk6',  name: 'JWT + Refresh Token',     icon: '🔐', level: 4, category: 'Security',       status: 'strong',   desc: 'Đã implement: access token 15m, refresh token rotation, revoke.', notes: '' },
    { id: 'sk7',  name: 'ReactJS / TypeScript',    icon: '⚛️', level: 1, category: 'Frontend',       status: 'gap',      desc: 'Ưu tiên cao nhất. Frontend scaffold xong, chưa có LoginPage.', notes: '' },
    { id: 'sk8',  name: 'TailwindCSS',             icon: '🎨', level: 2, category: 'Frontend',       status: 'learning', desc: 'Đã cài trong frontend. Cần thực hành với component thực tế.', notes: '' },
    { id: 'sk9',  name: 'Redis Caching',           icon: '⚡', level: 2, category: 'Infrastructure', status: 'learning', desc: 'Docker Redis đang chạy. Chưa tích hợp vào business features.', notes: '' },
    { id: 'sk10', name: 'Docker / Compose',        icon: '🐳', level: 3, category: 'DevOps',         status: 'learning', desc: 'Multi-container stack chạy ổn. SQL Server + Redis hoạt động.', notes: '' },
    { id: 'sk11', name: 'Semantic Kernel / AI',    icon: '🤖', level: 0, category: 'AI',             status: 'gap',      desc: 'Chưa học. Sẽ bắt đầu ở Sprint 4-5 sau khi hệ thống base xong.', notes: '' },
    { id: 'sk12', name: 'GitHub Actions / CI/CD',  icon: '🚀', level: 0, category: 'DevOps',         status: 'gap',      desc: 'Chưa học. Bước cuối sau khi mọi thứ hoạt động ổn định.', notes: '' },
  ],
  roadmap: [
    { id: 'rm1', step: 1, name: 'Clean Architecture .NET 8',                     resource: 'github.com/jasontaylordev/CleanArchitecture', duration: '3–5 ngày', priority: 'high', status: 'done',   notes: 'Đang áp dụng thực tế trong SmartShop — cấu trúc 4 layers ổn định.' },
    { id: 'rm2', step: 2, name: 'ReactJS Fundamentals (useState, useEffect, Router)', resource: 'react.dev (official) · Scrimba React course',  duration: '1–2 tuần', priority: 'high', status: 'active', notes: '' },
    { id: 'rm3', step: 3, name: 'JWT Authentication .NET — từ đầu đến cuối',        resource: 'YouTube: ASP.NET Core JWT Auth 2024',         duration: '2–3 ngày', priority: 'high', status: 'done',   notes: 'Đã implement access token + refresh token rotation + revoke endpoint.' },
    { id: 'rm4', step: 4, name: 'Redis với StackExchange.Redis — Caching Strategy', resource: 'docs.redis.io + Microsoft docs',              duration: '2–3 ngày', priority: 'mid',  status: 'todo',   notes: '' },
    { id: 'rm5', step: 5, name: 'Docker Compose cơ bản — Multi-container setup',   resource: 'docs.docker.com/compose',                     duration: '3–4 ngày', priority: 'mid',  status: 'done',   notes: 'SQL Server + Redis chạy ổn. docker-compose.yml hoàn chỉnh.' },
    { id: 'rm6', step: 6, name: 'Semantic Kernel + OpenAI API',                     resource: 'learn.microsoft.com/semantic-kernel',         duration: '1 tuần',   priority: 'low',  status: 'todo',   notes: '' },
    { id: 'rm7', step: 7, name: 'GitHub Actions CI/CD + Azure App Service Deploy',  resource: 'github.com/features/actions · Azure docs',   duration: '2–3 ngày', priority: 'low',  status: 'todo',   notes: '' },
  ],
  notes: [],
  lastUpdated: new Date().toISOString(),
};

// ── STATE ─────────────────────────────────────────────
let S = {};
const STORAGE_KEY = 'smartshop-tracker-v6';

function load() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    S = raw ? JSON.parse(raw) : clone(DEFAULT_DATA);
  } catch {
    S = clone(DEFAULT_DATA);
  }
}

function save() {
  S.lastUpdated = new Date().toISOString();
  localStorage.setItem(STORAGE_KEY, JSON.stringify(S));
  updateBadges();
}

function clone(obj) { return JSON.parse(JSON.stringify(obj)); }

// ── HELPERS ───────────────────────────────────────────
function uid() { return 'id-' + Date.now() + '-' + Math.random().toString(36).slice(2, 7); }

function nowStr() {
  return new Date().toLocaleDateString('vi-VN', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric',
  });
}

function timeStr() {
  return new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

function escHtml(s) {
  return String(s)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;')
    .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

// ── CALCULATIONS ──────────────────────────────────────
function sprintPct(sprint) {
  const tasks = sprint.tasks;
  if (!tasks.length) return 0;
  const score = tasks.reduce((acc, t) => acc + (t.status === 'done' ? 1 : t.status === 'partial' ? 0.5 : 0), 0);
  return Math.round(score / tasks.length * 100);
}

function overallPct() {
  const all = S.sprints.flatMap(s => s.tasks);
  if (!all.length) return 0;
  const score = all.reduce((acc, t) => acc + (t.status === 'done' ? 1 : t.status === 'partial' ? 0.5 : 0), 0);
  return Math.round(score / all.length * 100);
}

function taskCounts() {
  const all = S.sprints.flatMap(s => s.tasks);
  return {
    total:   all.length,
    done:    all.filter(t => t.status === 'done').length,
    partial: all.filter(t => t.status === 'partial').length,
    todo:    all.filter(t => t.status === 'todo').length,
  };
}

// ── RING SVG ──────────────────────────────────────────
function ring(pct, size = 72, sw = 7, color = '') {
  const r = (size - sw * 2) / 2;
  const circ = +(2 * Math.PI * r).toFixed(2);
  const offset = +(circ * (1 - Math.min(pct, 100) / 100)).toFixed(2);
  const cx = size / 2;
  const c = color || pctColor(pct);
  return `
    <div class="ring-wrap" style="width:${size}px;height:${size}px;">
      <svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}" style="transform:rotate(-90deg)">
        <circle cx="${cx}" cy="${cx}" r="${r}" fill="none" stroke="var(--surface2)" stroke-width="${sw}"/>
        <circle cx="${cx}" cy="${cx}" r="${r}" fill="none" stroke="${c}" stroke-width="${sw}"
          stroke-linecap="round" stroke-dasharray="${circ}" stroke-dashoffset="${offset}"/>
      </svg>
      <div class="ring-label">
        <span class="ring-pct" style="font-size:${size/5}px">${pct}%</span>
      </div>
    </div>`;
}

function pctColor(p) {
  if (p >= 100) return '#22c55e';
  if (p >= 60)  return '#eab308';
  if (p >= 20)  return '#6c63ff';
  return '#ef4444';
}

// ── STATUS ────────────────────────────────────────────
const STATUS_CYCLE = { todo: 'partial', partial: 'done', done: 'todo' };
const STATUS_ICON  = { done: '✅', partial: '🟡', todo: '⬜', active: '🔄' };
const STATUS_LABEL = { done: 'Xong', partial: 'Một phần', todo: 'Chưa làm', active: 'Đang làm' };

function statusBadge(s) {
  const cls = { done: 'badge-done', active: 'badge-active', todo: 'badge-todo' }[s] || 'badge-todo';
  const map = { done: 'DONE', active: 'IN PROGRESS', todo: 'PENDING' };
  return `<span class="sprint-badge ${cls}">${map[s] || s.toUpperCase()}</span>`;
}

function priorityBadge(p) {
  const cls = { high: 'priority-high', mid: 'priority-mid', low: 'priority-low' }[p];
  const lbl = { high: '🔴 Cao', mid: '🟠 Vừa', low: '🟢 Thấp' }[p] || p;
  return `<span class="priority-badge ${cls}">${lbl}</span>`;
}

function skillStatusBadge(s) {
  const cls = { strong: 'badge-strong', learning: 'badge-learning', gap: 'badge-gap' }[s];
  const lbl = { strong: '⭐ Vững', learning: '📚 Đang học', gap: '❌ Cần học' }[s] || s;
  return `<span class="skill-status-badge ${cls}" title="Click để đổi trạng thái">${lbl}</span>`;
}

// ── NAVIGATION ────────────────────────────────────────
let currentView = 'dashboard';

function navigate(view) {
  currentView = view;
  document.querySelectorAll('.view').forEach(el => el.classList.remove('active'));
  document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
  const viewEl = document.getElementById(`view-${view}`);
  if (viewEl) viewEl.classList.add('active');
  const navEl = document.querySelector(`[data-view="${view}"]`);
  if (navEl) navEl.classList.add('active');
  document.getElementById('sidebar').classList.remove('open');
  renderView(view);
}

function renderView(v) {
  ({ dashboard: renderDashboard, sprints: renderSprints, skills: renderSkills,
     roadmap: renderRoadmap, notes: renderNotes, settings: renderSettings })[v]?.();
}

// ── BADGES ────────────────────────────────────────────
function updateBadges() {
  const active = S.sprints.filter(s => s.status === 'active').length;
  const el = document.getElementById('badge-active');
  if (el) el.textContent = active || '';

  const nb = document.getElementById('badge-notes');
  if (nb) nb.textContent = S.notes.length || '';
}

// ══════════════════════════════════════════════════════
// RENDER: DASHBOARD
// ══════════════════════════════════════════════════════
function renderDashboard() {
  const pct    = overallPct();
  const counts = taskCounts();
  const doneSp = S.sprints.filter(s => s.status === 'done').length;
  const activeSp = S.sprints.find(s => s.status === 'active');
  const skillGaps = S.skills.filter(s => s.status === 'gap').length;

  const el = document.getElementById('view-dashboard');
  el.innerHTML = `
    <div class="view-header">
      <div>
        <h1 class="view-title">📊 Dashboard</h1>
        <p class="view-sub">${escHtml(S.profile.name)} · ${escHtml(S.profile.project)} · ${escHtml(S.profile.role)}</p>
      </div>
      <div class="view-date">${nowStr()}</div>
    </div>

    <div class="stats-row">
      <div class="stat-card c-purple">
        <div class="stat-label">Tổng tiến độ</div>
        <div class="stat-val" style="color:#a78bfa">${pct}%</div>
        <div class="stat-sub">${counts.done} task đã xong / ${counts.total} tổng</div>
      </div>
      <div class="stat-card c-teal">
        <div class="stat-label">Sprint hoàn thành</div>
        <div class="stat-val" style="color:#2dd4bf">${doneSp} / ${S.sprints.length}</div>
        <div class="stat-sub">${S.sprints.filter(s => s.status === 'active').length} sprint đang chạy</div>
      </div>
      <div class="stat-card c-yellow">
        <div class="stat-label">Task xong / tổng</div>
        <div class="stat-val" style="color:#fbbf24">${counts.done} / ${counts.total}</div>
        <div class="stat-sub">${counts.partial} đang làm · ${counts.todo} chưa làm</div>
      </div>
      <div class="stat-card c-red">
        <div class="stat-label">Kỹ năng cần học</div>
        <div class="stat-val" style="color:#f87171">${skillGaps}</div>
        <div class="stat-sub">${S.skills.filter(s => s.status === 'strong').length} kỹ năng vững</div>
      </div>
    </div>

    <div class="dash-grid">

      <!-- Overall progress -->
      <div class="card dash-left" style="grid-row:1">
        <div class="card-title">
          Tiến Độ Tổng Quan
          <button class="card-title-action" onclick="navigate('sprints')">Chi tiết →</button>
        </div>
        <div class="big-ring-area">
          ${ring(pct, 120, 12)}
          <div class="big-ring-bars">
            ${S.sprints.map(sp => {
              const p = sprintPct(sp);
              return `
                <div class="bar-row">
                  <div class="bar-label-row">
                    <span class="name">${escHtml(sp.name.split('—')[0].trim())}</span>
                    <span>${p}%</span>
                  </div>
                  <div class="bar-track">
                    <div class="bar-fill" style="width:${p}%;background:${sp.color}"></div>
                  </div>
                </div>`;
            }).join('')}
          </div>
        </div>
      </div>

      <!-- Active Sprint -->
      <div class="card dash-right" style="grid-row:1">
        <div class="card-title">
          Sprint Đang Chạy
          <button class="card-title-action" onclick="navigate('sprints')">Xem →</button>
        </div>
        ${activeSp ? `
          <div style="font-weight:700;margin-bottom:4px">${escHtml(activeSp.name)}</div>
          <div style="font-size:0.8rem;color:${activeSp.color};margin-bottom:10px">${sprintPct(activeSp)}% hoàn thành</div>
          <div class="bar-track" style="margin-bottom:16px">
            <div class="bar-fill" style="width:${sprintPct(activeSp)}%;background:${activeSp.color}"></div>
          </div>
          <div>
            ${activeSp.tasks.map(t => `
              <div class="active-task-mini ${t.status}">
                <span>${STATUS_ICON[t.status]}</span>
                <span class="t-name" style="font-size:0.8rem;flex:1;word-break:break-word">${escHtml(t.name)}</span>
              </div>`).join('')}
          </div>
        ` : `<div class="empty-state"><div class="empty-icon">🎉</div><p>Không có sprint nào đang chạy.</p></div>`}
      </div>

      <!-- Skills summary -->
      <div class="card dash-left" style="grid-row:2">
        <div class="card-title">
          Kỹ Năng
          <button class="card-title-action" onclick="navigate('skills')">Quản lý →</button>
        </div>
        ${S.skills.map(sk => `
          <div class="skill-mini-row">
            <span style="font-size:1rem">${sk.icon}</span>
            <span style="flex:1;font-size:0.8rem">${escHtml(sk.name)}</span>
            <div class="skill-mini-stars">
              ${[1,2,3,4,5].map(i => `<div class="star-xs ${i<=sk.level?'on':''}"></div>`).join('')}
            </div>
            <div class="s-dot ${sk.status}"></div>
          </div>`).join('')}
      </div>

      <!-- Recent notes -->
      <div class="card dash-right" style="grid-row:2">
        <div class="card-title">
          Ghi Chú Gần Đây
          <button class="card-title-action" onclick="navigate('notes')">Xem tất cả →</button>
        </div>
        ${S.notes.length === 0
          ? `<div class="empty-state" style="padding:24px 0">
               <div class="empty-icon">📝</div>
               <p>Chưa có ghi chú nào.</p>
               <button class="btn outline sm" onclick="navigate('notes')">Thêm ghi chú</button>
             </div>`
          : [...S.notes].reverse().slice(0, 3).map(n => `
              <div class="note-card" style="margin-bottom:10px">
                <div class="note-card-head">
                  <span class="note-date">${escHtml(n.date)}</span>
                  ${n.tag ? `<span class="note-tag">${escHtml(n.tag)}</span>` : ''}
                </div>
                <div class="note-text" style="font-size:0.8rem">${escHtml(n.content.slice(0,120))}${n.content.length>120?'…':''}</div>
              </div>`).join('')
        }
      </div>
    </div>`;
}

// ══════════════════════════════════════════════════════
// RENDER: SPRINTS
// ══════════════════════════════════════════════════════
function renderSprints() {
  const el = document.getElementById('view-sprints');
  el.innerHTML = `
    <div class="view-header">
      <div>
        <h1 class="view-title">🏃 Sprints</h1>
        <p class="view-sub">Quản lý tiến độ từng sprint và task</p>
      </div>
      <div class="view-actions">
        <button class="btn primary" id="btn-add-sprint">+ Thêm Sprint</button>
      </div>
    </div>

    <div class="info-banner">
      💡 Click vào icon trạng thái (⬜ 🟡 ✅) để cập nhật tiến độ task. Click tên sprint để mở rộng.
    </div>

    <div class="sprint-list" id="sprint-list">
      ${S.sprints.map(sp => renderSprintCard(sp)).join('')}
    </div>`;

  document.getElementById('btn-add-sprint').onclick = () => openAddSprintModal();
  bindSprintEvents();
}

function renderSprintCard(sp) {
  const pct = sprintPct(sp);
  return `
    <div class="sprint-card" data-sprint-id="${sp.id}">
      <div class="sprint-head" data-toggle="${sp.id}">
        <div class="sprint-color-bar" style="background:${sp.color}"></div>
        <div class="sprint-head-info">
          <div class="sprint-head-name">${escHtml(sp.name)}</div>
          <div class="sprint-head-meta">${sp.date ? escHtml(sp.date) : 'Chưa có ngày'} · ${sp.tasks.length} tasks · ${pct}%</div>
        </div>
        ${ring(pct, 50, 5, sp.color)}
        ${statusBadge(sp.status)}
        <div class="sprint-head-actions">
          <button class="btn ghost icon sm" title="Chỉnh sửa sprint" data-action="edit-sprint" data-sid="${sp.id}">✏️</button>
          <button class="btn ghost icon sm" title="Xóa sprint" data-action="del-sprint" data-sid="${sp.id}">🗑️</button>
        </div>
        <span class="sprint-chevron">▼</span>
      </div>
      <div class="sprint-body" id="body-${sp.id}">
        <div class="sprint-progress-row">
          <div class="bar-track"><div class="bar-fill" style="width:${pct}%;background:${sp.color}"></div></div>
          <span class="sprint-pct-label">${pct}%</span>
        </div>
        <div class="task-list" id="tasks-${sp.id}">
          ${sp.tasks.map(t => renderTaskRow(sp.id, t)).join('')}
        </div>
        <button class="add-task-btn" data-action="add-task" data-sid="${sp.id}">+ Thêm task</button>
      </div>
    </div>`;
}

function renderTaskRow(sid, t) {
  return `
    <div class="task-item ${t.status}" data-task-id="${t.id}" data-sid="${sid}">
      <button class="task-status-btn" title="Click để đổi trạng thái" data-action="toggle-task" data-sid="${sid}" data-tid="${t.id}">
        ${STATUS_ICON[t.status]}
      </button>
      <div class="task-body">
        <span class="task-name">${escHtml(t.name)}</span>
        ${t.notes ? `<div class="task-notes-inline">💬 ${escHtml(t.notes)}</div>` : ''}
      </div>
      <div class="task-actions">
        <button class="btn ghost icon sm" title="Chỉnh sửa" data-action="edit-task" data-sid="${sid}" data-tid="${t.id}">✏️</button>
        <button class="btn ghost icon sm" title="Xóa" data-action="del-task" data-sid="${sid}" data-tid="${t.id}">🗑️</button>
      </div>
    </div>`;
}

function bindSprintEvents() {
  // Toggle accordion
  document.querySelectorAll('.sprint-head[data-toggle]').forEach(head => {
    head.addEventListener('click', function(e) {
      if (e.target.closest('[data-action]')) return;
      const sid   = this.dataset.toggle;
      const body  = document.getElementById(`body-${sid}`);
      const chev  = this.querySelector('.sprint-chevron');
      const isOpen = body.classList.contains('open');
      body.classList.toggle('open', !isOpen);
      this.classList.toggle('open', !isOpen);
      if (chev) chev.classList.toggle('open', !isOpen);
    });
  });

  // Delegated action clicks
  document.getElementById('sprint-list').addEventListener('click', handleSprintAction);
}

function handleSprintAction(e) {
  const btn = e.target.closest('[data-action]');
  if (!btn) return;
  const { action, sid, tid } = btn.dataset;

  if (action === 'toggle-task')  toggleTaskStatus(sid, tid);
  if (action === 'edit-task')    openEditTaskModal(sid, tid);
  if (action === 'del-task')     confirmDelete(() => deleteTask(sid, tid), 'task này');
  if (action === 'edit-sprint')  openEditSprintModal(sid);
  if (action === 'del-sprint')   confirmDelete(() => deleteSprint(sid), 'sprint này và tất cả tasks');
  if (action === 'add-task')     openAddTaskModal(sid);
}

// ── Sprint mutations
function toggleTaskStatus(sid, tid) {
  const sp = S.sprints.find(s => s.id === sid);
  const t  = sp?.tasks.find(t => t.id === tid);
  if (!t) return;
  t.status = STATUS_CYCLE[t.status] || 'todo';
  save();
  refreshSprintCard(sid);
  toast(`Task: ${STATUS_LABEL[t.status]}`, 'success');
}

function refreshSprintCard(sid) {
  const sp  = S.sprints.find(s => s.id === sid);
  if (!sp) return;
  const old = document.querySelector(`[data-sprint-id="${sid}"]`);
  if (!old) return;
  const wasOpen = document.getElementById(`body-${sid}`)?.classList.contains('open');
  const tmp = document.createElement('div');
  tmp.innerHTML = renderSprintCard(sp);
  const newCard = tmp.firstElementChild;
  old.replaceWith(newCard);
  if (wasOpen) {
    const body = document.getElementById(`body-${sid}`);
    const head = newCard.querySelector('.sprint-head');
    const chev = newCard.querySelector('.sprint-chevron');
    body?.classList.add('open');
    head?.classList.add('open');
    chev?.classList.add('open');
  }
  // Re-bind events on the new card
  const headEl = newCard.querySelector('.sprint-head[data-toggle]');
  headEl?.addEventListener('click', function(e) {
    if (e.target.closest('[data-action]')) return;
    const id   = this.dataset.toggle;
    const b    = document.getElementById(`body-${id}`);
    const ch   = this.querySelector('.sprint-chevron');
    b?.classList.toggle('open');
    this.classList.toggle('open');
    ch?.classList.toggle('open');
  });
  document.getElementById('sprint-list')?.removeEventListener('click', handleSprintAction);
  document.getElementById('sprint-list')?.addEventListener('click', handleSprintAction);
}

function deleteTask(sid, tid) {
  const sp = S.sprints.find(s => s.id === sid);
  if (!sp) return;
  sp.tasks = sp.tasks.filter(t => t.id !== tid);
  save();
  refreshSprintCard(sid);
  toast('Đã xóa task', 'success');
}

function deleteSprint(sid) {
  S.sprints = S.sprints.filter(s => s.id !== sid);
  save();
  renderSprints();
  toast('Đã xóa sprint', 'success');
}

// ── Modals: Sprint & Task
function openAddSprintModal() {
  openModal('+ Thêm Sprint Mới', `
    <div class="modal-field">
      <label class="modal-label">Tên sprint</label>
      <input class="modal-input" id="m-sname" placeholder="Sprint 5 — Tên sprint..." />
    </div>
    <div class="modal-field">
      <label class="modal-label">Trạng thái</label>
      <select class="modal-select" id="m-sstatus">
        <option value="todo">⏳ Chưa bắt đầu</option>
        <option value="active">🔄 Đang chạy</option>
        <option value="done">✅ Hoàn thành</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Màu đại diện</label>
      <input type="color" id="m-scolor" value="#6c63ff" style="width:60px;height:36px;border:none;border-radius:6px;cursor:pointer;background:transparent" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Ghi chú ngày</label>
      <input class="modal-input" id="m-sdate" placeholder="VD: 01/06/2026" />
    </div>
  `, () => {
    const name = document.getElementById('m-sname').value.trim();
    if (!name) { toast('Vui lòng nhập tên sprint', 'error'); return false; }
    S.sprints.push({
      id: uid(), name,
      status: document.getElementById('m-sstatus').value,
      color:  document.getElementById('m-scolor').value,
      date:   document.getElementById('m-sdate').value.trim(),
      tasks:  [],
    });
    save();
    renderSprints();
    toast('Đã thêm sprint mới', 'success');
  });
}

function openEditSprintModal(sid) {
  const sp = S.sprints.find(s => s.id === sid);
  if (!sp) return;
  openModal('✏️ Chỉnh Sửa Sprint', `
    <div class="modal-field">
      <label class="modal-label">Tên sprint</label>
      <input class="modal-input" id="m-sname" value="${escHtml(sp.name)}" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Trạng thái</label>
      <select class="modal-select" id="m-sstatus">
        <option value="todo"   ${sp.status==='todo'  ?'selected':''}>⏳ Chưa bắt đầu</option>
        <option value="active" ${sp.status==='active'?'selected':''}>🔄 Đang chạy</option>
        <option value="done"   ${sp.status==='done'  ?'selected':''}>✅ Hoàn thành</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Màu</label>
      <input type="color" id="m-scolor" value="${sp.color}" style="width:60px;height:36px;border:none;border-radius:6px;cursor:pointer;background:transparent" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Ngày</label>
      <input class="modal-input" id="m-sdate" value="${escHtml(sp.date)}" />
    </div>
  `, () => {
    const name = document.getElementById('m-sname').value.trim();
    if (!name) { toast('Vui lòng nhập tên sprint', 'error'); return false; }
    sp.name   = name;
    sp.status = document.getElementById('m-sstatus').value;
    sp.color  = document.getElementById('m-scolor').value;
    sp.date   = document.getElementById('m-sdate').value.trim();
    save();
    refreshSprintCard(sid);
    toast('Đã cập nhật sprint', 'success');
  });
}

function openAddTaskModal(sid) {
  openModal('+ Thêm Task', `
    <div class="modal-field">
      <label class="modal-label">Tên task</label>
      <input class="modal-input" id="m-tname" placeholder="Mô tả task cần làm..." />
    </div>
    <div class="modal-field">
      <label class="modal-label">Trạng thái ban đầu</label>
      <select class="modal-select" id="m-tstatus">
        <option value="todo">⬜ Chưa làm</option>
        <option value="partial">🟡 Đang làm</option>
        <option value="done">✅ Hoàn thành</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Ghi chú (tùy chọn)</label>
      <textarea class="modal-textarea" id="m-tnotes" placeholder="Ghi chú thêm về task..."></textarea>
    </div>
  `, () => {
    const name = document.getElementById('m-tname').value.trim();
    if (!name) { toast('Vui lòng nhập tên task', 'error'); return false; }
    const sp = S.sprints.find(s => s.id === sid);
    sp?.tasks.push({
      id: uid(), name,
      status: document.getElementById('m-tstatus').value,
      notes:  document.getElementById('m-tnotes').value.trim(),
    });
    save();
    refreshSprintCard(sid);
    toast('Đã thêm task', 'success');
  });
}

function openEditTaskModal(sid, tid) {
  const sp = S.sprints.find(s => s.id === sid);
  const t  = sp?.tasks.find(t => t.id === tid);
  if (!t) return;
  openModal('✏️ Chỉnh Sửa Task', `
    <div class="modal-field">
      <label class="modal-label">Tên task</label>
      <input class="modal-input" id="m-tname" value="${escHtml(t.name)}" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Trạng thái</label>
      <select class="modal-select" id="m-tstatus">
        <option value="todo"    ${t.status==='todo'   ?'selected':''}>⬜ Chưa làm</option>
        <option value="partial" ${t.status==='partial'?'selected':''}>🟡 Đang làm</option>
        <option value="done"    ${t.status==='done'   ?'selected':''}>✅ Hoàn thành</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Ghi chú</label>
      <textarea class="modal-textarea" id="m-tnotes">${escHtml(t.notes)}</textarea>
    </div>
  `, () => {
    const name = document.getElementById('m-tname').value.trim();
    if (!name) { toast('Vui lòng nhập tên task', 'error'); return false; }
    t.name   = name;
    t.status = document.getElementById('m-tstatus').value;
    t.notes  = document.getElementById('m-tnotes').value.trim();
    save();
    refreshSprintCard(sid);
    toast('Đã cập nhật task', 'success');
  });
}

// ══════════════════════════════════════════════════════
// RENDER: SKILLS
// ══════════════════════════════════════════════════════
let skillFilter = 'all';

function renderSkills() {
  const cats = ['all', ...new Set(S.skills.map(s => s.category))];
  const filtered = skillFilter === 'all' ? S.skills : S.skills.filter(s => s.category === skillFilter);

  const el = document.getElementById('view-skills');
  el.innerHTML = `
    <div class="view-header">
      <div>
        <h1 class="view-title">🎯 Kỹ Năng</h1>
        <p class="view-sub">Đánh giá và cập nhật mức độ thành thạo</p>
      </div>
      <div class="view-actions">
        <button class="btn primary" id="btn-add-skill">+ Thêm Kỹ Năng</button>
      </div>
    </div>

    <div class="info-banner">
      💡 Click vào ⭐ để cập nhật mức độ · Click badge trạng thái để đổi (Vững/Đang học/Cần học)
    </div>

    <div class="skills-filter" id="skill-filters">
      ${cats.map(c => `
        <button class="filter-btn ${skillFilter===c?'active':''}" data-cat="${c}">
          ${c === 'all' ? 'Tất cả' : c} ${c!=='all'?`(${S.skills.filter(s=>s.category===c).length})` : `(${S.skills.length})`}
        </button>`).join('')}
    </div>

    <div class="skills-grid" id="skills-grid">
      ${filtered.map(sk => renderSkillCard(sk)).join('')}
    </div>`;

  document.getElementById('btn-add-skill').onclick = () => openAddSkillModal();
  document.getElementById('skill-filters').addEventListener('click', e => {
    const btn = e.target.closest('.filter-btn');
    if (!btn) return;
    skillFilter = btn.dataset.cat;
    renderSkills();
  });
  document.getElementById('skills-grid').addEventListener('click', handleSkillAction);
}

function renderSkillCard(sk) {
  return `
    <div class="skill-card" data-skill-id="${sk.id}">
      <div class="skill-card-head">
        <span class="skill-icon">${sk.icon}</span>
        <div class="skill-info">
          <div class="skill-name">${escHtml(sk.name)}</div>
          <div class="skill-cat">${escHtml(sk.category)}</div>
        </div>
        ${skillStatusBadge(sk.status)}
      </div>
      <div class="skill-stars" data-action="rate" data-skid="${sk.id}">
        ${[1,2,3,4,5].map(i => `
          <button class="star ${i <= sk.level ? 'on' : ''}" data-star="${i}" title="${i}/5" aria-label="Mức ${i}">
            ${i <= sk.level ? '★' : '☆'}
          </button>`).join('')}
      </div>
      <div class="skill-desc">${escHtml(sk.desc)}</div>
      ${sk.notes ? `<div style="font-size:0.75rem;color:var(--accent2);margin-top:8px;font-style:italic">📌 ${escHtml(sk.notes)}</div>` : ''}
      <div class="skill-card-actions">
        <button class="btn ghost sm" data-action="edit-skill" data-skid="${sk.id}">✏️ Chỉnh sửa</button>
        <button class="btn ghost sm" data-action="del-skill"  data-skid="${sk.id}">🗑️ Xóa</button>
      </div>
    </div>`;
}

function handleSkillAction(e) {
  const rateArea = e.target.closest('[data-action="rate"]');
  if (rateArea) {
    const starBtn = e.target.closest('.star');
    if (starBtn) {
      const skid = rateArea.dataset.skid;
      const lvl  = parseInt(starBtn.dataset.star);
      const sk   = S.skills.find(s => s.id === skid);
      if (sk) {
        sk.level = sk.level === lvl ? lvl - 1 : lvl; // click same star = decrease
        save();
        renderSkills();
        toast(`${sk.name}: mức ${sk.level}/5`, 'success');
      }
    }
    return;
  }

  const badge = e.target.closest('.skill-status-badge');
  if (badge) {
    const card = badge.closest('[data-skill-id]');
    const sk   = S.skills.find(s => s.id === card?.dataset.skillId);
    if (sk) {
      const cycle = { strong: 'learning', learning: 'gap', gap: 'strong' };
      sk.status = cycle[sk.status] || 'learning';
      save();
      renderSkills();
      toast(`${sk.name}: ${sk.status}`, 'info');
    }
    return;
  }

  const btn = e.target.closest('[data-action]');
  if (!btn) return;
  const { action, skid } = btn.dataset;
  if (action === 'edit-skill') openEditSkillModal(skid);
  if (action === 'del-skill')  confirmDelete(() => deleteSkill(skid), 'kỹ năng này');
}

function deleteSkill(skid) {
  S.skills = S.skills.filter(s => s.id !== skid);
  save();
  renderSkills();
  toast('Đã xóa kỹ năng', 'success');
}

function openAddSkillModal() {
  openModal('+ Thêm Kỹ Năng', `
    <div class="modal-field">
      <label class="modal-label">Tên kỹ năng</label>
      <input class="modal-input" id="m-skname" placeholder="VD: MediatR, Azure..." />
    </div>
    <div class="modal-field">
      <label class="modal-label">Icon (emoji)</label>
      <input class="modal-input" id="m-skicon" value="📌" placeholder="Emoji..." style="width:80px" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Danh mục</label>
      <input class="modal-input" id="m-skcat" placeholder="Backend, Frontend, DevOps..." />
    </div>
    <div class="modal-field">
      <label class="modal-label">Mức độ (1–5)</label>
      <input type="range" id="m-sklevel" min="0" max="5" value="1" style="width:100%" />
      <div id="m-sklevel-lbl" style="font-size:0.75rem;color:var(--muted);margin-top:4px">Mức 1/5</div>
    </div>
    <div class="modal-field">
      <label class="modal-label">Trạng thái</label>
      <select class="modal-select" id="m-skstatus">
        <option value="gap">❌ Cần học</option>
        <option value="learning">📚 Đang học</option>
        <option value="strong">⭐ Vững</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Mô tả</label>
      <textarea class="modal-textarea" id="m-skdesc" placeholder="Mô tả ngắn về kỹ năng..."></textarea>
    </div>
  `, () => {
    const name = document.getElementById('m-skname').value.trim();
    if (!name) { toast('Vui lòng nhập tên kỹ năng', 'error'); return false; }
    S.skills.push({
      id:       uid(),
      name,
      icon:     document.getElementById('m-skicon').value.trim() || '📌',
      category: document.getElementById('m-skcat').value.trim() || 'Khác',
      level:    parseInt(document.getElementById('m-sklevel').value),
      status:   document.getElementById('m-skstatus').value,
      desc:     document.getElementById('m-skdesc').value.trim(),
      notes:    '',
    });
    save();
    renderSkills();
    toast('Đã thêm kỹ năng', 'success');
  });
  // Range label live update
  setTimeout(() => {
    const r = document.getElementById('m-sklevel');
    const l = document.getElementById('m-sklevel-lbl');
    r?.addEventListener('input', () => { if (l) l.textContent = `Mức ${r.value}/5`; });
  }, 50);
}

function openEditSkillModal(skid) {
  const sk = S.skills.find(s => s.id === skid);
  if (!sk) return;
  openModal('✏️ Chỉnh Sửa Kỹ Năng', `
    <div class="modal-field">
      <label class="modal-label">Tên kỹ năng</label>
      <input class="modal-input" id="m-skname" value="${escHtml(sk.name)}" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Icon</label>
      <input class="modal-input" id="m-skicon" value="${escHtml(sk.icon)}" style="width:80px" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Danh mục</label>
      <input class="modal-input" id="m-skcat" value="${escHtml(sk.category)}" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Mức độ (1–5): <span id="m-sklevel-lbl">Mức ${sk.level}/5</span></label>
      <input type="range" id="m-sklevel" min="0" max="5" value="${sk.level}" style="width:100%" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Trạng thái</label>
      <select class="modal-select" id="m-skstatus">
        <option value="gap"      ${sk.status==='gap'     ?'selected':''}>❌ Cần học</option>
        <option value="learning" ${sk.status==='learning'?'selected':''}>📚 Đang học</option>
        <option value="strong"   ${sk.status==='strong'  ?'selected':''}>⭐ Vững</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Mô tả</label>
      <textarea class="modal-textarea" id="m-skdesc">${escHtml(sk.desc)}</textarea>
    </div>
    <div class="modal-field">
      <label class="modal-label">Ghi chú thêm</label>
      <input class="modal-input" id="m-sknotes" value="${escHtml(sk.notes)}" placeholder="Ghi chú cá nhân..." />
    </div>
  `, () => {
    const name = document.getElementById('m-skname').value.trim();
    if (!name) { toast('Vui lòng nhập tên kỹ năng', 'error'); return false; }
    sk.name     = name;
    sk.icon     = document.getElementById('m-skicon').value.trim() || sk.icon;
    sk.category = document.getElementById('m-skcat').value.trim() || sk.category;
    sk.level    = parseInt(document.getElementById('m-sklevel').value);
    sk.status   = document.getElementById('m-skstatus').value;
    sk.desc     = document.getElementById('m-skdesc').value.trim();
    sk.notes    = document.getElementById('m-sknotes').value.trim();
    save();
    renderSkills();
    toast('Đã cập nhật kỹ năng', 'success');
  });
  setTimeout(() => {
    const r = document.getElementById('m-sklevel');
    const l = document.getElementById('m-sklevel-lbl');
    r?.addEventListener('input', () => { if (l) l.textContent = `Mức ${r.value}/5`; });
  }, 50);
}

// ══════════════════════════════════════════════════════
// RENDER: ROADMAP
// ══════════════════════════════════════════════════════
function renderRoadmap() {
  const el = document.getElementById('view-roadmap');
  el.innerHTML = `
    <div class="view-header">
      <div>
        <h1 class="view-title">🗺️ Lộ Trình Học</h1>
        <p class="view-sub">Các chủ đề cần học theo thứ tự ưu tiên</p>
      </div>
      <div class="view-actions">
        <button class="btn primary" id="btn-add-rm">+ Thêm Mục</button>
      </div>
    </div>

    <div class="info-banner">
      💡 Click vào số thứ tự để đổi trạng thái (Chưa làm → Đang học → Hoàn thành)
    </div>

    <div class="roadmap-list" id="roadmap-list">
      ${S.roadmap.map(item => renderRoadmapItem(item)).join('')}
    </div>`;

  document.getElementById('btn-add-rm').onclick = () => openAddRoadmapModal();
  document.getElementById('roadmap-list').addEventListener('click', handleRoadmapAction);
}

function renderRoadmapItem(item) {
  const nodeClass = item.status === 'done' ? 'done' : item.status === 'active' ? 'active' : 'todo';
  const nodeIcon  = item.status === 'done' ? '✓' : item.status === 'active' ? '▶' : item.step;
  return `
    <div class="roadmap-item" data-rm-id="${item.id}">
      <div class="roadmap-node ${nodeClass}" data-action="toggle-rm" data-rmid="${item.id}" title="Click để đổi trạng thái">
        ${nodeIcon}
      </div>
      <div class="roadmap-card ${nodeClass}">
        <div class="roadmap-card-head">
          <span class="roadmap-name">${escHtml(item.name)}</span>
          ${priorityBadge(item.priority)}
        </div>
        <div class="roadmap-card-meta">
          <span>⏱ ${escHtml(item.duration)}</span>
          <span>📚 ${escHtml(item.resource)}</span>
        </div>
        ${item.notes ? `<div class="roadmap-notes">💬 ${escHtml(item.notes)}</div>` : ''}
        <div class="roadmap-card-actions">
          <button class="btn ghost sm" data-action="edit-rm" data-rmid="${item.id}">✏️ Chỉnh sửa</button>
          <button class="btn ghost sm" data-action="del-rm"  data-rmid="${item.id}">🗑️ Xóa</button>
        </div>
      </div>
    </div>`;
}

function handleRoadmapAction(e) {
  const btn = e.target.closest('[data-action]');
  if (!btn) return;
  const { action, rmid } = btn.dataset;
  if (action === 'toggle-rm') toggleRoadmapStatus(rmid);
  if (action === 'edit-rm')   openEditRoadmapModal(rmid);
  if (action === 'del-rm')    confirmDelete(() => deleteRoadmap(rmid), 'mục lộ trình này');
}

function toggleRoadmapStatus(rmid) {
  const item = S.roadmap.find(r => r.id === rmid);
  if (!item) return;
  const cycle = { todo: 'active', active: 'done', done: 'todo' };
  item.status = cycle[item.status] || 'todo';
  save();
  renderRoadmap();
  toast(`Trạng thái: ${STATUS_LABEL[item.status] || item.status}`, 'info');
}

function deleteRoadmap(rmid) {
  S.roadmap = S.roadmap.filter(r => r.id !== rmid);
  save();
  renderRoadmap();
  toast('Đã xóa mục', 'success');
}

function roadmapModalFields(item) {
  const v = item || {};
  return `
    <div class="modal-field">
      <label class="modal-label">Chủ đề học</label>
      <input class="modal-input" id="m-rmname" value="${escHtml(v.name||'')}" placeholder="VD: Clean Architecture .NET 8" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Tài nguyên</label>
      <input class="modal-input" id="m-rmres" value="${escHtml(v.resource||'')}" placeholder="URL hoặc tên tài liệu..." />
    </div>
    <div class="modal-field">
      <label class="modal-label">Thời gian ước tính</label>
      <input class="modal-input" id="m-rmdur" value="${escHtml(v.duration||'')}" placeholder="VD: 3–5 ngày" />
    </div>
    <div class="modal-field">
      <label class="modal-label">Độ ưu tiên</label>
      <select class="modal-select" id="m-rmpri">
        <option value="high" ${(v.priority||'')===  'high'?'selected':''}>🔴 Cao</option>
        <option value="mid"  ${(v.priority||'')==='mid' ?'selected':''}>🟠 Vừa</option>
        <option value="low"  ${(v.priority||'')==='low' ?'selected':''}>🟢 Thấp</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Trạng thái</label>
      <select class="modal-select" id="m-rmst">
        <option value="todo"   ${(v.status||'')==='todo'  ?'selected':''}>⏳ Chưa học</option>
        <option value="active" ${(v.status||'')==='active'?'selected':''}>▶ Đang học</option>
        <option value="done"   ${(v.status||'')==='done'  ?'selected':''}>✓ Hoàn thành</option>
      </select>
    </div>
    <div class="modal-field">
      <label class="modal-label">Ghi chú</label>
      <textarea class="modal-textarea" id="m-rmnotes">${escHtml(v.notes||'')}</textarea>
    </div>`;
}

function openAddRoadmapModal() {
  openModal('+ Thêm Mục Học', roadmapModalFields(), () => {
    const name = document.getElementById('m-rmname').value.trim();
    if (!name) { toast('Vui lòng nhập chủ đề', 'error'); return false; }
    S.roadmap.push({
      id: uid(), step: S.roadmap.length + 1, name,
      resource: document.getElementById('m-rmres').value.trim(),
      duration: document.getElementById('m-rmdur').value.trim(),
      priority: document.getElementById('m-rmpri').value,
      status:   document.getElementById('m-rmst').value,
      notes:    document.getElementById('m-rmnotes').value.trim(),
    });
    save();
    renderRoadmap();
    toast('Đã thêm mục học', 'success');
  });
}

function openEditRoadmapModal(rmid) {
  const item = S.roadmap.find(r => r.id === rmid);
  if (!item) return;
  openModal('✏️ Chỉnh Sửa Mục Học', roadmapModalFields(item), () => {
    const name = document.getElementById('m-rmname').value.trim();
    if (!name) { toast('Vui lòng nhập chủ đề', 'error'); return false; }
    item.name     = name;
    item.resource = document.getElementById('m-rmres').value.trim();
    item.duration = document.getElementById('m-rmdur').value.trim();
    item.priority = document.getElementById('m-rmpri').value;
    item.status   = document.getElementById('m-rmst').value;
    item.notes    = document.getElementById('m-rmnotes').value.trim();
    save();
    renderRoadmap();
    toast('Đã cập nhật', 'success');
  });
}

// ══════════════════════════════════════════════════════
// RENDER: NOTES
// ══════════════════════════════════════════════════════
function renderNotes() {
  const el = document.getElementById('view-notes');
  el.innerHTML = `
    <div class="view-header">
      <div>
        <h1 class="view-title">📝 Ghi Chú</h1>
        <p class="view-sub">${S.notes.length} ghi chú · Nhật ký học tập</p>
      </div>
    </div>

    <div class="note-compose">
      <div class="card-title" style="margin-bottom:10px">✍️ Thêm Ghi Chú Mới</div>
      <textarea id="note-input" placeholder="Ghi lại những gì bạn học được hôm nay, vấn đề gặp phải, hoặc kế hoạch tiếp theo..."></textarea>
      <div class="note-compose-actions">
        <select id="note-tag" class="modal-select" style="width:auto;padding:7px 10px">
          <option value="">Không có tag</option>
          <option value="Sprint 0">Sprint 0</option>
          <option value="Sprint 1">Sprint 1</option>
          <option value="Sprint 2">Sprint 2</option>
          <option value="Sprint 3">Sprint 3</option>
          <option value="Sprint 4-5">Sprint 4-5</option>
          <option value="Kỹ năng">Kỹ năng</option>
          <option value="Vấn đề">Vấn đề</option>
          <option value="Ý tưởng">Ý tưởng</option>
          <option value="Review">Review</option>
        </select>
        <button class="btn ghost" id="btn-clear-note">Xóa trắng</button>
        <button class="btn primary" id="btn-save-note">💾 Lưu Ghi Chú</button>
      </div>
    </div>

    ${S.notes.length === 0
      ? `<div class="empty-state"><div class="empty-icon">📭</div><p>Chưa có ghi chú nào. Hãy ghi lại tiến trình học tập!</p></div>`
      : `<div class="notes-list" id="notes-list">
          ${[...S.notes].reverse().map(n => `
            <div class="note-card" data-note-id="${n.id}">
              <div class="note-card-head">
                <span class="note-date">🕐 ${escHtml(n.date)}</span>
                <div style="display:flex;gap:6px;align-items:center">
                  ${n.tag ? `<span class="note-tag">${escHtml(n.tag)}</span>` : ''}
                  <button class="btn ghost icon sm" data-action="del-note" data-nid="${n.id}" title="Xóa ghi chú">🗑️</button>
                </div>
              </div>
              <div class="note-text">${escHtml(n.content)}</div>
            </div>`).join('')}
        </div>`
    }`;

  document.getElementById('btn-save-note').onclick = saveNote;
  document.getElementById('btn-clear-note').onclick = () => {
    document.getElementById('note-input').value = '';
  };
  document.getElementById('note-input').addEventListener('keydown', e => {
    if (e.ctrlKey && e.key === 'Enter') saveNote();
  });
  document.getElementById('notes-list')?.addEventListener('click', e => {
    const btn = e.target.closest('[data-action="del-note"]');
    if (btn) confirmDelete(() => deleteNote(btn.dataset.nid), 'ghi chú này');
  });
}

function saveNote() {
  const content = document.getElementById('note-input').value.trim();
  if (!content) { toast('Vui lòng nhập nội dung ghi chú', 'error'); return; }
  const tag = document.getElementById('note-tag').value;
  S.notes.push({
    id:      uid(),
    date:    new Date().toLocaleString('vi-VN'),
    content, tag,
  });
  save();
  renderNotes();
  toast('Đã lưu ghi chú', 'success');
}

function deleteNote(nid) {
  S.notes = S.notes.filter(n => n.id !== nid);
  save();
  renderNotes();
  toast('Đã xóa ghi chú', 'success');
}

// ══════════════════════════════════════════════════════
// RENDER: SETTINGS
// ══════════════════════════════════════════════════════
function renderSettings() {
  const el = document.getElementById('view-settings');
  const lu = S.lastUpdated ? new Date(S.lastUpdated).toLocaleString('vi-VN') : 'Chưa rõ';
  el.innerHTML = `
    <div class="view-header">
      <div>
        <h1 class="view-title">⚙️ Cài Đặt</h1>
        <p class="view-sub">Cập nhật hồ sơ · Export/Import dữ liệu</p>
      </div>
    </div>

    <!-- Profile -->
    <div class="settings-section">
      <div class="settings-title">👤 Hồ Sơ Học Viên</div>
      <div class="form-row">
        <span class="form-label">Họ và tên</span>
        <input class="form-input" id="cfg-name"   value="${escHtml(S.profile.name)}" />
      </div>
      <div class="form-row">
        <span class="form-label">Dự án</span>
        <input class="form-input" id="cfg-project" value="${escHtml(S.profile.project)}" />
      </div>
      <div class="form-row">
        <span class="form-label">Vai trò / Nền tảng</span>
        <input class="form-input" id="cfg-role"    value="${escHtml(S.profile.role)}" />
      </div>
      <div class="form-row">
        <span class="form-label">Ngày bắt đầu</span>
        <input class="form-input" type="date" id="cfg-start"  value="${S.profile.startDate}" />
      </div>
      <div class="form-row">
        <span class="form-label">Ngày mục tiêu</span>
        <input class="form-input" type="date" id="cfg-target" value="${S.profile.targetDate}" />
      </div>
      <div style="margin-top:14px">
        <button class="btn primary" id="btn-save-profile">💾 Lưu Hồ Sơ</button>
      </div>
    </div>

    <!-- Stats -->
    <div class="settings-section">
      <div class="settings-title">📊 Thống Kê Dữ Liệu</div>
      <div style="display:grid;grid-template-columns:repeat(auto-fit,minmax(140px,1fr));gap:12px">
        ${[
          ['Sprint', S.sprints.length],
          ['Tasks', S.sprints.flatMap(s=>s.tasks).length],
          ['Kỹ năng', S.skills.length],
          ['Lộ trình', S.roadmap.length],
          ['Ghi chú', S.notes.length],
        ].map(([k,v]) => `
          <div style="background:var(--surface2);border:1px solid var(--border);border-radius:8px;padding:12px;text-align:center">
            <div style="font-size:1.6rem;font-weight:800">${v}</div>
            <div style="font-size:0.72rem;color:var(--muted)">${k}</div>
          </div>`).join('')}
      </div>
      <div style="margin-top:14px;font-size:0.75rem;color:var(--muted)">
        Cập nhật lần cuối: ${lu}
      </div>
    </div>

    <!-- Data management -->
    <div class="settings-section">
      <div class="settings-title">💾 Quản Lý Dữ Liệu</div>
      <div class="settings-actions">
        <button class="btn success" id="btn-export">⬇️ Xuất JSON</button>
        <button class="btn outline" id="btn-import-btn">⬆️ Nhập JSON</button>
        <input type="file" id="btn-import" accept=".json" style="display:none" />
        <button class="btn danger" id="btn-reset">🔄 Reset về mặc định</button>
      </div>
      <div style="margin-top:12px;font-size:0.78rem;color:var(--muted)">
        ⚠️ Reset sẽ xóa tất cả dữ liệu tùy chỉnh và khôi phục dữ liệu mặc định SmartShop.
      </div>
    </div>

    <!-- Keyboard shortcuts -->
    <div class="settings-section">
      <div class="settings-title">⌨️ Phím Tắt</div>
      <div style="display:flex;flex-direction:column;gap:8px;font-size:0.82rem">
        ${[
          ['Ctrl + 1', 'Mở Dashboard'],
          ['Ctrl + 2', 'Mở Sprints'],
          ['Ctrl + 3', 'Mở Kỹ Năng'],
          ['Ctrl + 4', 'Mở Lộ Trình'],
          ['Ctrl + 5', 'Mở Ghi Chú'],
          ['Ctrl + Enter', 'Lưu ghi chú (khi đang nhập)'],
        ].map(([k, v]) => `
          <div style="display:flex;align-items:center;gap:12px">
            <code style="background:var(--surface2);border:1px solid var(--border);padding:3px 8px;border-radius:5px;font-size:0.75rem;min-width:100px;text-align:center">${k}</code>
            <span style="color:var(--muted)">${v}</span>
          </div>`).join('')}
      </div>
    </div>`;

  document.getElementById('btn-save-profile').onclick = () => {
    S.profile.name        = document.getElementById('cfg-name').value.trim() || S.profile.name;
    S.profile.project     = document.getElementById('cfg-project').value.trim() || S.profile.project;
    S.profile.role        = document.getElementById('cfg-role').value.trim();
    S.profile.startDate   = document.getElementById('cfg-start').value;
    S.profile.targetDate  = document.getElementById('cfg-target').value;
    save();
    toast('Đã lưu hồ sơ', 'success');
  };

  document.getElementById('btn-export').onclick = () => {
    const blob = new Blob([JSON.stringify(S, null, 2)], { type: 'application/json' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href = url;
    a.download = `smartshop-tracker-${new Date().toISOString().slice(0,10)}.json`;
    a.click();
    URL.revokeObjectURL(url);
    toast('Đã xuất dữ liệu', 'success');
  };

  document.getElementById('btn-import-btn').onclick = () => {
    document.getElementById('btn-import').click();
  };

  document.getElementById('btn-import').onchange = e => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = ev => {
      try {
        const data = JSON.parse(ev.target.result);
        if (!data.sprints || !data.skills) { toast('File JSON không hợp lệ', 'error'); return; }
        S = data;
        save();
        renderView(currentView);
        toast('Đã nhập dữ liệu thành công', 'success');
      } catch {
        toast('Lỗi đọc file JSON', 'error');
      }
    };
    reader.readAsText(file);
  };

  document.getElementById('btn-reset').onclick = () => {
    confirmDelete(() => {
      S = clone(DEFAULT_DATA);
      save();
      renderView(currentView);
      toast('Đã reset về dữ liệu mặc định', 'info');
    }, 'toàn bộ dữ liệu và reset về mặc định');
    document.getElementById('confirm-ok').textContent = 'Reset';
  };
}

// ══════════════════════════════════════════════════════
// MODAL SYSTEM
// ══════════════════════════════════════════════════════
let _modalConfirmCb = null;

function openModal(title, bodyHTML, onConfirm) {
  document.getElementById('modal-title').textContent = title;
  document.getElementById('modal-body').innerHTML = bodyHTML;
  _modalConfirmCb = onConfirm;
  document.getElementById('modal-overlay').classList.remove('hidden');
  setTimeout(() => {
    const first = document.querySelector('#modal-body input, #modal-body textarea, #modal-body select');
    first?.focus();
  }, 60);
}

function closeModal() {
  document.getElementById('modal-overlay').classList.add('hidden');
  _modalConfirmCb = null;
}

function setupModal() {
  document.getElementById('modal-close-btn').onclick = closeModal;
  document.getElementById('modal-cancel').onclick    = closeModal;
  document.getElementById('modal-ok').onclick = () => {
    if (_modalConfirmCb) {
      const result = _modalConfirmCb();
      if (result !== false) closeModal();
    } else {
      closeModal();
    }
  };
  document.getElementById('modal-overlay').addEventListener('click', e => {
    if (e.target === document.getElementById('modal-overlay')) closeModal();
  });
  document.getElementById('modal-box').addEventListener('keydown', e => {
    if (e.key === 'Escape') closeModal();
    if (e.key === 'Enter' && e.target.tagName !== 'TEXTAREA') {
      e.preventDefault();
      document.getElementById('modal-ok').click();
    }
  });
}

// ══════════════════════════════════════════════════════
// CONFIRM SYSTEM
// ══════════════════════════════════════════════════════
let _confirmCb = null;

function confirmDelete(cb, what) {
  document.getElementById('confirm-msg').textContent = `Bạn có chắc chắn muốn xóa ${what}? Hành động này không thể hoàn tác.`;
  document.getElementById('confirm-ok').textContent = 'Xóa';
  _confirmCb = cb;
  document.getElementById('confirm-overlay').classList.remove('hidden');
}

function setupConfirm() {
  document.getElementById('confirm-cancel').onclick = () => {
    document.getElementById('confirm-overlay').classList.add('hidden');
    _confirmCb = null;
  };
  document.getElementById('confirm-ok').onclick = () => {
    if (_confirmCb) _confirmCb();
    document.getElementById('confirm-overlay').classList.add('hidden');
    _confirmCb = null;
  };
}

// ══════════════════════════════════════════════════════
// TOAST SYSTEM
// ══════════════════════════════════════════════════════
function toast(msg, type = 'info') {
  const icon = { success: '✅', error: '❌', info: 'ℹ️' }[type] || 'ℹ️';
  const el   = document.createElement('div');
  el.className = `toast ${type}`;
  el.innerHTML = `<span>${icon}</span><span>${escHtml(msg)}</span>`;
  document.getElementById('toasts').appendChild(el);
  setTimeout(() => {
    el.classList.add('out');
    setTimeout(() => el.remove(), 280);
  }, 2800);
}

// ══════════════════════════════════════════════════════
// CLOCK
// ══════════════════════════════════════════════════════
function updateClock() {
  const el = document.getElementById('sidebar-clock');
  if (el) {
    el.innerHTML = `<div>${new Date().toLocaleDateString('vi-VN', {weekday:'short',day:'2-digit',month:'2-digit'})}</div>
                    <div style="font-size:1rem;font-weight:700">${timeStr()}</div>`;
  }
}

// ══════════════════════════════════════════════════════
// KEYBOARD SHORTCUTS
// ══════════════════════════════════════════════════════
function setupKeyboard() {
  document.addEventListener('keydown', e => {
    if (!e.ctrlKey) return;
    const views = ['', 'dashboard', 'sprints', 'skills', 'roadmap', 'notes'];
    const num = parseInt(e.key);
    if (num >= 1 && num <= 5 && views[num]) {
      e.preventDefault();
      navigate(views[num]);
    }
  });
}

// ══════════════════════════════════════════════════════
// INIT
// ══════════════════════════════════════════════════════
function init() {
  load();

  // Navigation
  document.querySelectorAll('.nav-item').forEach(btn => {
    btn.addEventListener('click', () => navigate(btn.dataset.view));
  });

  // Mobile sidebar toggle
  document.getElementById('menu-toggle').addEventListener('click', () => {
    document.getElementById('sidebar').classList.toggle('open');
  });

  // Close sidebar on outside click
  document.addEventListener('click', e => {
    const sidebar = document.getElementById('sidebar');
    const toggle  = document.getElementById('menu-toggle');
    if (sidebar.classList.contains('open') && !sidebar.contains(e.target) && !toggle.contains(e.target)) {
      sidebar.classList.remove('open');
    }
  });

  setupModal();
  setupConfirm();
  setupKeyboard();

  // Clock
  updateClock();
  setInterval(updateClock, 30000);

  // Initial render
  updateBadges();
  renderView(currentView);

  // Open sprint 1 by default when navigating to sprints
  const origNavigate = navigate;
  window.navigate = function(view) {
    origNavigate(view);
  };
}

init();
