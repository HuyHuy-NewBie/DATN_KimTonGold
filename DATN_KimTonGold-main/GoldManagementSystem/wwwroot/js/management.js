(function () {
  const host = document.getElementById("managementDashboard");
  if (!host) return;

  const menu = document.querySelector("[data-management-tree]");
  const menuTrigger = document.querySelector("[data-management-menu-trigger]");
  let requestController;
  let isSubmitting = false;

  const closeMenu = () => {
    if (!menu || !menuTrigger) return;
    menu.classList.remove("is-open");
    menuTrigger.setAttribute("aria-expanded", "false");
  };

  const showLoadError = message => {
    host.querySelector("[data-dashboard-load-error]")?.remove();
    const alert = document.createElement("div");
    alert.className = "management-alert management-alert--danger";
    alert.dataset.dashboardLoadError = "true";
    alert.textContent = message;
    host.prepend(alert);
  };

  const cleanUrl = rawUrl => {
    const url = new URL(rawUrl, window.location.origin);
    url.searchParams.delete("partial");
    return url;
  };

  const buildFormData = (form, submitter) => submitter
    ? new FormData(form, submitter)
    : new FormData(form);

  const extractDashboard = html => {
    const documentFragment = new DOMParser().parseFromString(html, "text/html");
    return documentFragment.querySelector("#managementDashboard")?.innerHTML ?? html;
  };

  const syncMenuState = rawUrl => {
    if (!menu) return;
    const current = cleanUrl(rawUrl);
    const currentTab = current.searchParams.get("tab") || "overview";
    const currentSubtab = current.searchParams.get("subtab") || "";
    let currentBranch = current.searchParams.get("branchId") || "";
    if (!currentBranch && !["users", "permissions", "branches", "audit"].includes(currentTab)) {
      const selectedBranchLink = menu.querySelector(".management-menu--active a[data-management-branch-name]")
        || menu.querySelector("a[data-management-branch-name]");
      currentBranch = selectedBranchLink
        ? cleanUrl(selectedBranchLink.href).searchParams.get("branchId") || ""
        : "";
    }
    let activeLink;

    menu.querySelectorAll("a[data-dashboard-link]").forEach(link => {
      const target = cleanUrl(link.href);
      const targetTab = target.searchParams.get("tab") || "overview";
      const targetSubtab = target.searchParams.get("subtab") || "";
      const targetBranch = target.searchParams.get("branchId") || "";
      const subtabMatches = currentTab !== "warehouse" || targetSubtab === currentSubtab;
      const branchMatches = link.hasAttribute("data-management-system-link")
        ? !currentBranch
        : (!currentBranch || targetBranch === currentBranch);
      const isActive = targetTab === currentTab && subtabMatches && branchMatches;
      link.classList.toggle("is-active", isActive);
      if (isActive && !activeLink) activeLink = link;
    });

    menu.querySelectorAll(".management-menu").forEach(branch => {
      const containsActive = Boolean(branch.querySelector("a.is-active"));
      branch.classList.toggle("management-menu--active", containsActive);
      if (containsActive) branch.open = true;
    });
  };

  const afterDashboardRender = () => {
    const updated = host.querySelector("[data-permission-updated='true']");
    if (updated && !sessionStorage.getItem("permission-reload-confirmed")) {
      window.alert("Quyền đã được cập nhật. Nhấn OK để tải lại và áp dụng menu mới.");
      sessionStorage.setItem("permission-reload-confirmed", "1");
      window.location.reload();
      return;
    }
    if (!updated) sessionStorage.removeItem("permission-reload-confirmed");
  };

  const replaceDashboard = (html, rawUrl, pushState) => {
    host.innerHTML = extractDashboard(html);
    const url = cleanUrl(rawUrl);
    host.dataset.partialUrl = `${url.pathname}${url.search}`;
    if (pushState) history.pushState({}, "", url);
    syncMenuState(url);
    afterDashboardRender();
  };

  const loadDashboard = async (rawUrl, pushState) => {
    if (isSubmitting) return;
    const displayUrl = cleanUrl(rawUrl);
    const requestUrl = new URL(displayUrl);
    requestUrl.searchParams.set("partial", "true");
    requestController?.abort();
    requestController = new AbortController();
    const timeout = window.setTimeout(() => requestController.abort(), 5000);
    host.classList.add("is-loading");
    closeMenu();
    try {
      const response = await fetch(requestUrl, {
        cache: "no-store",
        credentials: "same-origin",
        headers: { "X-Requested-With": "XMLHttpRequest" },
        signal: requestController.signal
      });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      replaceDashboard(await response.text(), displayUrl, pushState);
    } catch (error) {
      if (error.name !== "AbortError")
        showLoadError("Không thể tải dashboard. Vui lòng thử lại mà không cần làm mới toàn bộ trang.");
    } finally {
      window.clearTimeout(timeout);
      host.classList.remove("is-loading");
    }
  };

  const submitDashboardForm = async (form, submitter) => {
    if (isSubmitting) return;
    const action = new URL(form.action, window.location.origin);
    const method = (form.method || "get").toLowerCase();
    if (method === "get") {
      const data = buildFormData(form, submitter);
      data.forEach((value, key) => action.searchParams.set(key, value));
      await loadDashboard(action, true);
      return;
    }

    isSubmitting = true;
    requestController?.abort();
    requestController = new AbortController();
    const timeout = window.setTimeout(() => requestController.abort(), 10000);
    const button = submitter instanceof HTMLElement ? submitter : null;
    if (button) button.setAttribute("disabled", "disabled");
    host.classList.add("is-loading");
    try {
      const response = await fetch(action, {
        method: method.toUpperCase(),
        body: buildFormData(form, submitter),
        cache: "no-store",
        credentials: "same-origin",
        headers: { "X-Requested-With": "XMLHttpRequest" },
        signal: requestController.signal
      });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      const responseUrl = response.redirected ? response.url : action;
      replaceDashboard(await response.text(), responseUrl, true);
    } catch (error) {
      if (error.name !== "AbortError")
        showLoadError("Thao tác chưa hoàn tất. Vui lòng kiểm tra dữ liệu và thử lại.");
    } finally {
      window.clearTimeout(timeout);
      host.classList.remove("is-loading");
      if (button?.isConnected) button.removeAttribute("disabled");
      isSubmitting = false;
    }
  };

  document.addEventListener("click", event => {
    const trigger = event.target.closest("[data-management-menu-trigger]");
    if (trigger && menu) {
      event.preventDefault();
      const willOpen = !menu.classList.contains("is-open");
      menu.classList.toggle("is-open", willOpen);
      trigger.setAttribute("aria-expanded", String(willOpen));
      return;
    }

    const dashboardLink = event.target.closest("a[data-dashboard-link]");
    if (dashboardLink
      && !event.defaultPrevented
      && event.button === 0
      && !event.ctrlKey
      && !event.metaKey
      && !event.shiftKey
      && !event.altKey) {
      const target = new URL(dashboardLink.href, window.location.origin);
      if (target.origin === window.location.origin && target.pathname.toLowerCase() === "/management") {
        event.preventDefault();
        loadDashboard(target, true);
        return;
      }
    }

    const auditButton = event.target.closest("[data-audit-switch]");
    if (auditButton && host.contains(auditButton)) {
      host.querySelectorAll("[data-audit-switch]").forEach(item => item.classList.toggle("is-active", item === auditButton));
      host.querySelectorAll("[data-audit-panel]").forEach(panel => panel.classList.toggle("is-active", panel.dataset.auditPanel === auditButton.dataset.auditSwitch));
      return;
    }

    if (menu && !menu.contains(event.target)) closeMenu();
  });

  host.addEventListener("submit", event => {
    if (event.defaultPrevented) return;
    const form = event.target.closest("form");
    if (!form) return;
    const action = new URL(form.action, window.location.origin);
    const isDashboardGet = form.matches("[data-dashboard-form]");
    const isManagementPost = (form.method || "get").toLowerCase() === "post"
      && action.origin === window.location.origin
      && action.pathname.toLowerCase().startsWith("/management/");
    if (!isDashboardGet && !isManagementPost) return;
    event.preventDefault();
    submitDashboardForm(form, event.submitter);
  });

  document.addEventListener("keydown", event => {
    if (event.key === "Escape") {
      closeMenu();
      menuTrigger?.focus();
    }
  });

  syncMenuState(window.location.href);
  afterDashboardRender();
  window.addEventListener("popstate", () => loadDashboard(window.location.href, false));
  window.setInterval(() => {
    if (!isSubmitting && !host.querySelector("input:focus, textarea:focus, select:focus, details[open] form"))
      loadDashboard(window.location.href, false);
  }, 60000);
})();
