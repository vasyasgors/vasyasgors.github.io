document.addEventListener("DOMContentLoaded", () => {
  const navItems = [
    { href: "index.html", label: "Главная" },
    //{ href: "portfolio-games.html", label: "Портфолио игры" },
    { href: "portfolio-3d.html", label: "Портфолио 3D" },
    { href: "materials-unity.html", label: "Материалы по Unity" },
    { href: "materials-blender.html", label: "Материалы по Blender" },
    { href: "materials-programming.html", label: "Материалы по программированию" },
  ];

  const nav = document.createElement("nav");
  const ul = document.createElement("ul");

  // Текущий путь без параметров и без слеша в конце
  let currentPath = window.location.pathname.split('?')[0].replace(/\/$/, '').toLowerCase();

  // Если мы на главной странице, currentPath обычно '/'
  if (currentPath === '/') currentPath = '/index.html';

  navItems.forEach(({ href, label }) => {
    const li = document.createElement("li");
    const a = document.createElement("a");
    a.href = href;
    a.textContent = label;

    // Нормализуем href для сравнения
    let hrefPath = href.toLowerCase();
    if (!hrefPath.startsWith('/')) {
      hrefPath = '/' + hrefPath;
    }
    hrefPath = hrefPath.replace(/\/$/, '');

    // Активируем, если совпадает или текущий путь начинается с hrefPath (для вложенных страниц)
    if (currentPath === hrefPath || currentPath.startsWith(hrefPath.replace(/\.html$/, ''))) {
      a.classList.add("active");
      a.setAttribute("aria-current", "page");
    }
    li.appendChild(a);
    ul.appendChild(li);
  });

  nav.appendChild(ul);

  // Вставляем навигацию в контейнер с id="nav-container"
  const navContainer = document.getElementById("nav-container");
  if (navContainer) {
    navContainer.appendChild(nav);
  }
});