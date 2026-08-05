document.addEventListener("DOMContentLoaded", () => {
  const menuPanel = document.querySelector("[data-menu-panel]");
  const searchPanel = document.querySelector("[data-search-panel]");

  const syncOverlayState = () => {
    const hasOpenOverlay = [menuPanel, searchPanel].some(
      (panel) => panel?.classList.contains("is-open")
    );

    document.body.style.overflow = hasOpenOverlay ? "hidden" : "";
    document.body.classList.toggle("panel-open", hasOpenOverlay);
  };

  const setOpenState = (panel, isOpen) => {
    if (!panel) {
      return;
    }

    panel.classList.toggle("is-open", isOpen);
    syncOverlayState();
  };

  document.querySelectorAll("[data-toggle-menu]").forEach((button) => {
    button.addEventListener("click", () => setOpenState(menuPanel, true));
  });

  document.querySelectorAll("[data-close-menu]").forEach((button) => {
    button.addEventListener("click", () => setOpenState(menuPanel, false));
  });

  document.querySelectorAll("[data-toggle-search]").forEach((button) => {
    button.addEventListener("click", () => setOpenState(searchPanel, true));
  });

  document.querySelectorAll("[data-close-search]").forEach((button) => {
    button.addEventListener("click", () => setOpenState(searchPanel, false));
  });

  document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") {
      return;
    }

    setOpenState(menuPanel, false);
    setOpenState(searchPanel, false);
  });

  document.querySelectorAll("[data-close-promo]").forEach((button) => {
    button.addEventListener("click", () => {
      const strip = button.closest(".promo-strip");
      if (strip) {
        strip.style.display = "none";
      }
    });
  });

  const runCarousel = (root, interval = 5000) => {
    const slides = Array.from(root.querySelectorAll(":scope > *"));
    if (slides.length <= 1) {
      return;
    }

    let activeIndex = slides.findIndex((slide) => slide.classList.contains("is-active"));
    if (activeIndex < 0) {
      activeIndex = 0;
      slides[0].classList.add("is-active");
    }

    const bullets = root.parentElement?.querySelectorAll("[data-carousel-bullet]") ?? [];

    const sync = () => {
      slides.forEach((slide, index) => {
        slide.classList.toggle("is-active", index === activeIndex);
      });

      bullets.forEach((bullet, index) => {
        bullet.classList.toggle("is-active", index === activeIndex);
      });
    };

    const next = () => {
      activeIndex = (activeIndex + 1) % slides.length;
      sync();
    };

    bullets.forEach((bullet, index) => {
      bullet.addEventListener("click", () => {
        activeIndex = index;
        sync();
      });
    });

    sync();
    window.setInterval(next, interval);
  };

  const runRotatingGallery = (root) => {
    const slides = Array.from(root.querySelectorAll(":scope > img"));
    if (!slides.length) {
      return;
    }

    const shouldRotate = root.dataset.rotatingGallery === "true";
    const rotationInterval = Math.max(Number(root.dataset.rotationInterval || 3600), 2200);
    const thumbs = Array.from(
      root.closest(".details-gallery")?.querySelectorAll("[data-gallery-thumb]") ?? []
    );

    let activeIndex = slides.findIndex((slide) => slide.classList.contains("is-active"));
    if (activeIndex < 0) {
      activeIndex = 0;
      slides[0].classList.add("is-active");
    }
    let timerId = null;

    const sync = () => {
      slides.forEach((slide, index) => {
        slide.classList.toggle("is-active", index === activeIndex);
      });

      thumbs.forEach((thumb, index) => {
        const isActive = index === activeIndex;
        thumb.classList.toggle("is-active", isActive);
        thumb.setAttribute("aria-current", isActive ? "true" : "false");
      });
    };

    const startRotation = () => {
      if (timerId !== null) {
        window.clearInterval(timerId);
        timerId = null;
      }

      if (!shouldRotate || slides.length <= 1) {
        return;
      }

      timerId = window.setInterval(() => {
        activeIndex = (activeIndex + 1) % slides.length;
        sync();
      }, rotationInterval);
    };

    thumbs.forEach((thumb, index) => {
      thumb.addEventListener("click", (event) => {
        event.preventDefault();
        activeIndex = index;
        sync();
        startRotation();
      });
    });

    sync();
    startRotation();
  };

  const getActiveLanguage = () =>
    document.documentElement.lang === "en" ? "en" : "vi";

  const translations = {
    vi: {
      updating: "Đang cập nhật",
      vndRateUpdating: "Tỷ giá VND đang cập nhật",
      source: "Nguồn",
      price: "Giá",
      buy: "Mua vào",
      sell: "Bán ra",
      updated: "Cập nhật",
      refresh: "Làm mới",
      details: "chi tiết",
      descriptionUpdating: "Đang cập nhật mô tả.",
      noSourceData: "Chưa có dữ liệu nguồn giá chi tiết.",
      spotSuffix: "giao ngay",
      changeLabel: "Biến động",
      days7: "7 ngày",
      days30: "30 ngày",
      spread: "Độ lệch",
      age: "Độ trễ",
      minutes: "phút",
      hoursPerRefresh: "giờ/lần",
      minutesPerRefresh: "phút/lần",
      silverBoard: "Bảng giá bạc",
      silverPulse: "Biến động bạc",
      goldBoard: "Bảng giá vàng",
      goldPulse: "Biến động vàng",
      silver: "Bạc",
      gold: "Vàng",
      marketUpdating: "Đang cập nhật dữ liệu thị trường.",
      marketLoadFailed: "Không thể tải bảng giá lúc này. Hệ thống sẽ thử lại sau.",
      marketLoadHint: "Kiểm tra kết nối mạng hoặc tải lại trang.",
      productInfo: "Thông tin sản phẩm",
      relatedProducts: "Sản phẩm liên quan",
      category: "Danh mục",
      material: "Chất liệu",
      weight: "Trọng lượng",
      processingFee: "Phí chế tác",
      status: "Trạng thái",
      branch: "Chi nhánh"
    },
    en: {
      updating: "Updating",
      vndRateUpdating: "VND exchange rate updating",
      source: "Source",
      price: "Price",
      buy: "Buy",
      sell: "Sell",
      updated: "Updated",
      refresh: "Refresh",
      details: "details",
      descriptionUpdating: "Description is being updated.",
      noSourceData: "Detailed source data is not available yet.",
      spotSuffix: "spot",
      changeLabel: "Movement",
      days7: "7 days",
      days30: "30 days",
      spread: "Spread",
      age: "Latency",
      minutes: "min",
      hoursPerRefresh: "hrs/refresh",
      minutesPerRefresh: "min/refresh",
      silverBoard: "Silver board",
      silverPulse: "Silver pulse",
      goldBoard: "Gold board",
      goldPulse: "Gold pulse",
      silver: "Silver",
      gold: "Gold",
      marketUpdating: "Market data is updating.",
      marketLoadFailed: "Unable to load market prices right now. The system will retry shortly.",
      marketLoadHint: "Check your network connection or refresh the page.",
      productInfo: "Product information",
      relatedProducts: "Related products",
      category: "Category",
      material: "Material",
      weight: "Weight",
      processingFee: "Processing fee",
      status: "Status",
      branch: "Branch"
    }
  };

  const t = (key) => translations[getActiveLanguage()][key] ?? translations.vi[key] ?? key;

    const exactAutoTranslations = {
    "Tất cả": "All",
    "Mới": "New",
    "Nhẫn cưới": "Wedding rings",
    "Dây chuyền": "Necklaces",
    "Lắc tay": "Bracelets",
    "Hoa tai": "Earrings",
    "Trang sức vàng": "Gold jewelry",
    "Trang sức bạc": "Silver jewelry",
    "-- Tất cả --": "-- All --",
    "-- Tất cả danh mục --": "-- All categories --",
    "-- Tất cả chất liệu --": "-- All materials --",
    "Quản Trị Viên": "Administrator",
    "Trụ sở chính": "Head office",
    "Mua ngay": "Shop now",
    "Xem ngay": "Discover now"
  };

  const tokenAutoTranslations = [
    ["Nhẫn Cưới", "Wedding Ring"],
    ["Dây Chuyền", "Necklace"],
    ["Lắc Tay", "Bracelet"],
    ["Bông Tai", "Earrings"],
    ["Kiềng cổ", "Collar Necklace"],
    ["Bộ Sưu Tập", "Collection"],
    ["Trang Sức Bạc", "Silver Jewelry"],
    ["Vàng 24K", "24K Gold"],
    ["Vàng 18K", "18K Gold"],
    ["Vàng 9999", "9999 Gold"],
    ["Vàng Trắng", "White Gold"],
    ["Vàng Ý 750", "750 Italian Gold"],
    ["Bạc S925", "S925 Silver"],
    ["Bạc Ý 925", "925 Italian Silver"],
    ["Bạc Ta", "Fine Silver"],
    ["Bạc Thái", "Thai Silver"],
    ["Còn hàng", "In stock"],
    ["Hết hàng", "Out of stock"],
    ["Đã bán", "Sold"],
    ["Bán chạy", "Best seller"],
    ["Phú Quý", "Prosperity"],
    ["Tài Lộc", "Fortune"],
    ["Hiện Đại", "Modern"],
    ["Trẻ Trung", "Youthful"],
    ["Tinh Xảo", "Refined"],
    ["Cao Cấp", "Premium"],
    ["Sang Trọng", "Elegant"],
    ["Cổ Điển", "Classic"],
    ["Quý Tộc", "Noble"],
    ["Bản Nhỏ", "Petite"],
    ["Trái Tim", "Heart"],
    ["Vu Quy", "Bridal"],
    ["Chạm Khắc Tứ Linh", "Four Sacred Beasts Engraved"],
    ["Chi nhánh:", "Branch:"],
    ["Danh mục", "Category"],
    ["Chất liệu", "Material"],
    ["Trạng thái", "Status"],
    ["Thông tin sản phẩm", "Product information"],
    ["Sản phẩm liên quan", "Related products"]
  ].sort((left, right) => right[0].length - left[0].length);

  const normalizeText = (value) => String(value || "").replace(/\s+/g, " ").trim();

  const translateDynamicText = (value, language = getActiveLanguage()) => {
    const source = normalizeText(value);
    if (!source || language !== "en") {
      return source;
    }

    if (exactAutoTranslations[source]) {
      return exactAutoTranslations[source];
    }

    let translated = source;
    tokenAutoTranslations.forEach(([vi, en]) => {
      translated = translated.split(vi).join(en);
    });

    return translated;
  };

  const applyAutoTranslations = (language) => {
    document.querySelectorAll("[data-auto-i18n], option:not([data-i18n])").forEach((element) => {
      if (!element.dataset.autoI18nVi) {
        element.dataset.autoI18nVi = normalizeText(element.textContent);
      }

      const source = element.dataset.autoI18nVi;
      element.textContent = language === "en" ? translateDynamicText(source, language) : source;
    });
  };

  const getLocale = () => (getActiveLanguage() === "en" ? "en-US" : "vi-VN");

  const createNumberFormatter = (options) =>
    new Intl.NumberFormat(getLocale(), options);

  const formatRefreshLabel = (minutes) => {
    const normalized = Number(minutes || 60);
    const compactNumberFormatter = createNumberFormatter({
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    });

    if (normalized % 60 === 0) {
      return `${compactNumberFormatter.format(normalized / 60)} ${t("hoursPerRefresh")}`;
    }

    return `${compactNumberFormatter.format(normalized)} ${t("minutesPerRefresh")}`;
  };

  const normalizeCurrency = (currency) => {
    return String(currency || "VND").toUpperCase();
  };

  const formatSigned = (value, suffix = "") => {
    if (value === null || value === undefined || Number.isNaN(Number(value))) {
      return t("updating");
    }

    const normalizedValue = Number(value);
    const prefix = normalizedValue > 0 ? "+" : "";
    const compactNumberFormatter = createNumberFormatter({
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    });
    return `${prefix}${compactNumberFormatter.format(normalizedValue)}${suffix}`;
  };

  const formatPrice = (value, currency = "USD") => {
    if (value === null || value === undefined || Number(value) <= 0) {
      return t("updating");
    }

    const normalizedCurrency = normalizeCurrency(currency);
    if (normalizedCurrency === "VND") {
      const vndPriceFormatter = createNumberFormatter({
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
      });
      return `${vndPriceFormatter.format(Number(value))} ₫`;
    }

    const usdPriceFormatter = createNumberFormatter({
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
    return `${usdPriceFormatter.format(Number(value))} ${normalizedCurrency}`;
  };

  const formatZeroPrice = (currency = "USD") => {
    const normalizedCurrency = normalizeCurrency(currency);
    if (normalizedCurrency === "VND") {
      return "0 ₫";
    }

    const usdPriceFormatter = createNumberFormatter({
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
    return `${usdPriceFormatter.format(0)} ${normalizedCurrency}`;
  };

  const formatSignedPrice = (value, currency = "USD") => {
    if (value === null || value === undefined || Number.isNaN(Number(value))) {
      return t("updating");
    }

    const normalizedValue = Number(value);
    if (normalizedValue === 0) {
      return formatZeroPrice(currency);
    }

    const prefix = normalizedValue > 0 ? "+" : normalizedValue < 0 ? "-" : "";
    return `${prefix}${formatPrice(Math.abs(normalizedValue), currency)}`;
  };

  const formatDateTime = (value) => {
    if (!value) {
      return t("updating");
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return t("updating");
    }

    return new Intl.DateTimeFormat(getLocale(), {
      dateStyle: "short",
      timeStyle: "short",
      timeZone: "Asia/Ho_Chi_Minh"
    }).format(date);
  };

  const formatUnit = (unit, currency = "USD") => {
    if (!unit) {
      return "";
    }

    if (unit === "troy_oz") {
      return `${normalizeCurrency(currency)} / oz`;
    }

    return unit.replaceAll("_", " ");
  };

  const formatFxRate = (payload) => {
    if (!payload?.usdToVndRate || Number(payload.usdToVndRate) <= 0) {
      return t("vndRateUpdating");
    }

    return `${payload?.fxSourceName || "Frankfurter"} · 1 USD ≈ ${formatPrice(payload.usdToVndRate, "VND")}`;
  };

  const formatAge = (value) => {
    if (value === null || value === undefined) {
      return t("updating");
    }

    const compactNumberFormatter = createNumberFormatter({
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    });
    return `${compactNumberFormatter.format(Number(value))} ${t("minutes")}`;
  };

  const resolveChange = (metal, key) => {
    return metal?.[key] ?? metal?.[key.toLowerCase()] ?? { amount: 0, percent: 0 };
  };

  const cardToneClass = (value) => {
    const normalized = Number(value || 0);
    if (normalized > 0) {
      return "is-up";
    }

    if (normalized < 0) {
      return "is-down";
    }

    return "is-flat";
  };

  const renderSpotCard = (metal, themeLabel) => {
    const change24H = resolveChange(metal, "change24H");
    const displayName = translateDynamicText(metal?.displayName || "");

    return `
      <div class="market-card__eyebrow">
        <span>${themeLabel}</span>
        <strong>${metal?.symbol || ""}</strong>
      </div>
      <h3>${displayName} ${t("spotSuffix")}</h3>
      <div class="market-card__value">${formatPrice(metal?.price, metal?.currency)}</div>
      <p class="market-card__caption">${formatUnit(metal?.unit, metal?.currency)} · ${t("updated")} ${formatDateTime(metal?.lastUpdatedUtc)}</p>
      <div class="market-card__stats">
        <div>
          <span>${t("buy")}</span>
          <strong>${formatPrice(metal?.bid, metal?.currency)}</strong>
        </div>
        <div>
          <span>${t("sell")}</span>
          <strong>${formatPrice(metal?.ask, metal?.currency)}</strong>
        </div>
        <div class="${cardToneClass(change24H?.percent)}">
          <span>24h</span>
          <strong>${formatSigned(change24H?.percent, "%")}</strong>
        </div>
      </div>
    `;
  };

  const renderPulseCard = (metal, themeLabel) => {
    const change24H = resolveChange(metal, "change24H");
    const change7D = resolveChange(metal, "change7D");
    const change30D = resolveChange(metal, "change30D");
    const displayName = translateDynamicText(metal?.displayName?.toLowerCase() || "");

    return `
      <div class="market-card__eyebrow">
        <span>${themeLabel}</span>
        <strong>Pulse</strong>
      </div>
      <h3>${t("changeLabel")} ${displayName}</h3>
      <div class="market-card__trend-grid">
        <div class="${cardToneClass(change24H?.percent)}">
          <span>24h</span>
          <strong>${formatSignedPrice(change24H?.amount, metal?.currency)}</strong>
          <small>${formatSigned(change24H?.percent, "%")}</small>
        </div>
        <div class="${cardToneClass(change7D?.percent)}">
          <span>7 ngày</span>
          <strong>${formatSignedPrice(change7D?.amount, metal?.currency)}</strong>
          <small>${formatSigned(change7D?.percent, "%")}</small>
        </div>
        <div class="${cardToneClass(change30D?.percent)}">
          <span>30 ngày</span>
          <strong>${formatSignedPrice(change30D?.amount, metal?.currency)}</strong>
          <small>${formatSigned(change30D?.percent, "%")}</small>
        </div>
      </div>
      <div class="market-card__stats market-card__stats--compact">
        <div>
          <span>${t("spread")}</span>
          <strong>${formatSigned(metal?.spreadPercent, "%")}</strong>
        </div>
        <div>
          <span>${t("age")}</span>
          <strong>${formatAge(metal?.dataAgeMinutes)}</strong>
        </div>
      </div>
    `;
  };

  const renderMarketDetail = (metal, themeLabel) => {
    const change24H = resolveChange(metal, "change24H");
    const change7D = resolveChange(metal, "change7D");
    const change30D = resolveChange(metal, "change30D");
    const sources = Array.isArray(metal?.sources) ? metal.sources : [];
    const displayName = translateDynamicText(metal?.displayName || "");

    const sourceRows = sources.length
      ? sources
          .map((source) => {
            return `
              <tr>
                <td>${source.source || "-"}</td>
                <td>${formatPrice(source.price, metal?.currency)}</td>
                <td>${formatPrice(source.bid, metal?.currency)}</td>
                <td>${formatPrice(source.ask, metal?.currency)}</td>
                <td>${formatDateTime(source.timestampUtc)}</td>
              </tr>
            `;
          })
          .join("")
      : `
          <tr>
            <td colspan="5">${t("noSourceData")}</td>
          </tr>
        `;

    return `
      <div class="market-detail__header">
        <div>
          <span>${themeLabel}</span>
          <h3>${displayName} ${t("details")}</h3>
        </div>
        <strong>${formatPrice(metal?.price, metal?.currency)}</strong>
      </div>
      <p class="market-detail__description">${translateDynamicText(metal?.description || t("descriptionUpdating"))}</p>
      <div class="market-detail__summary">
        <div>
          <span>${t("buy")}</span>
          <strong>${formatPrice(metal?.bid, metal?.currency)}</strong>
        </div>
        <div>
          <span>${t("sell")}</span>
          <strong>${formatPrice(metal?.ask, metal?.currency)}</strong>
        </div>
        <div class="${cardToneClass(change24H?.percent)}">
          <span>24h</span>
          <strong>${formatSigned(change24H?.percent, "%")}</strong>
        </div>
        <div class="${cardToneClass(change7D?.percent)}">
          <span>7 ngày</span>
          <strong>${formatSigned(change7D?.percent, "%")}</strong>
        </div>
        <div class="${cardToneClass(change30D?.percent)}">
          <span>30 ngày</span>
          <strong>${formatSigned(change30D?.percent, "%")}</strong>
        </div>
        <div>
          <span>${t("spread")}</span>
          <strong>${formatSigned(metal?.spreadPercent, "%")}</strong>
        </div>
      </div>
      <div class="market-detail__table-wrap">
        <table class="market-detail__table">
          <thead>
            <tr>
              <th>${t("source")}</th>
              <th>${t("price")}</th>
              <th>${t("buy")}</th>
              <th>${t("sell")}</th>
              <th>${t("updated")}</th>
            </tr>
          </thead>
          <tbody>${sourceRows}</tbody>
        </table>
      </div>
    `;
  };

  const renderMarketBoard = (board, payload) => {
    const silver = payload?.silver || {};
    const gold = payload?.gold || {};
    const status = board.querySelector("[data-market-status]");
    const cards = {
      "silver-spot": board.querySelector("[data-market-card='silver-spot']"),
      "silver-pulse": board.querySelector("[data-market-card='silver-pulse']"),
      "gold-spot": board.querySelector("[data-market-card='gold-spot']"),
      "gold-pulse": board.querySelector("[data-market-card='gold-pulse']")
    };

    if (status) {
      const liveClass = payload?.isLive ? "is-live" : "is-stale";
      status.className = `market-board__status ${liveClass}`;
      status.innerHTML = `
        <span>${payload?.statusMessage || "Đang cập nhật dữ liệu thị trường."}</span>
        <strong>${payload?.sourceName || "SILV DATA"} · ${formatFxRate(payload)} · Làm mới ${formatRefreshLabel(payload?.refreshIntervalMinutes || 60)}</strong>
      `;
    }

    if (cards["silver-spot"]) {
      cards["silver-spot"].innerHTML = renderSpotCard(silver, t("silverBoard"));
    }

    if (cards["silver-pulse"]) {
      cards["silver-pulse"].innerHTML = renderPulseCard(silver, t("silverPulse"));
    }

    if (cards["gold-spot"]) {
      cards["gold-spot"].innerHTML = renderSpotCard(gold, t("goldBoard"));
    }

    if (cards["gold-pulse"]) {
      cards["gold-pulse"].innerHTML = renderPulseCard(gold, t("goldPulse"));
    }

    board.querySelectorAll("[data-market-detail]").forEach((detail) => {
      const key = detail.dataset.marketDetail;
      const metal = key === "silver" ? silver : gold;
      const label = key === "silver" ? t("silver") : t("gold");
      detail.innerHTML = renderMarketDetail(metal, label);
    });
  };

  const initializeMarketBoards = () => {
    document.querySelectorAll("[data-market-board]").forEach((board) => {
      const endpoint = board.dataset.marketEndpoint;
      if (!endpoint) {
        return;
      }

      let refreshTimerId = null;

      const scheduleRefresh = (minutes) => {
        if (refreshTimerId) {
          window.clearTimeout(refreshTimerId);
        }

        refreshTimerId = window.setTimeout(loadBoard, Math.max(5, Number(minutes || 60)) * 60 * 1000);
      };

      const loadBoard = async () => {
        try {
          const response = await fetch(endpoint, {
            headers: {
              "X-Requested-With": "XMLHttpRequest"
            }
          });

          if (!response.ok) {
            throw new Error(`Market endpoint failed: ${response.status}`);
          }

          const payload = await response.json();
          board._marketPayload = payload;
          renderMarketBoard(board, payload);
          scheduleRefresh(payload?.refreshIntervalMinutes || 60);
        } catch (error) {
          const status = board.querySelector("[data-market-status]");
          if (status) {
            status.className = "market-board__status is-stale";
            status.innerHTML = `
              <span>Không thể tải bảng giá lúc này. Hệ thống sẽ thử lại sau.</span>
              <strong>Kiểm tra kết nối mạng hoặc tải lại trang.</strong>
            `;
          }

          scheduleRefresh(5);
        }
      };

      loadBoard();
    });
  };

  document.querySelectorAll("[data-carousel='promo-strip']").forEach((carousel) => {
    const track = carousel.querySelector(".promo-strip__inner");
    if (track) {
      runCarousel(track, 1500);
    }
  });

  document.querySelectorAll("[data-carousel='home-stage']").forEach((carousel) => {
    const track = carousel.querySelector(".home-stage__rail");
    if (track) {
      runCarousel(track, 1500);
    }
  });

  document.querySelectorAll("[data-carousel='collection-stage']").forEach((carousel) => {
    const track = carousel.querySelector(".collection-stage__rail");
    if (track) {
      runCarousel(track, 1500);
    }
  });

  document.querySelectorAll("[data-rotating-gallery]").forEach((gallery) => {
    runRotatingGallery(gallery);
  });

  initializeMarketBoards();

  const applyLanguage = (language) => {
    const normalizedLanguage = language === "en" ? "en" : "vi";

    document.documentElement.lang = normalizedLanguage;
    document.body.dataset.language = normalizedLanguage;

    document.querySelectorAll("[data-i18n]").forEach((element) => {
      const value =
        normalizedLanguage === "en"
          ? element.dataset.i18nEn || element.dataset.i18nVi
          : element.dataset.i18nVi || element.dataset.i18nEn;

      if (!value) {
        return;
      }

      const mode = element.dataset.i18nAttr || "text";
      if (mode === "placeholder") {
        element.setAttribute("placeholder", value);
        return;
      }

      if (mode === "title") {
        element.setAttribute("title", value);
        return;
      }

      element.textContent = value;
    });

    const languageToggle = document.querySelector("[data-language-toggle]");
    if (languageToggle) {
      const tooltip =
        normalizedLanguage === "en" ? "Switch language" : "Chuyển đổi ngôn ngữ";

      languageToggle.setAttribute("title", tooltip);
      languageToggle.setAttribute("aria-label", tooltip);

      languageToggle
        .querySelectorAll(".language-toggle__option")
        .forEach((option) => {
          option.classList.toggle(
            "is-active",
            option.dataset.langCode === normalizedLanguage
          );
        });
    }

    applyAutoTranslations(normalizedLanguage);

    document.querySelectorAll("[data-market-board]").forEach((board) => {
      if (board._marketPayload) {
        renderMarketBoard(board, board._marketPayload);
      }
    });

    document.documentElement.classList.remove("lang-loading");
  };

  const savedLanguage = localStorage.getItem("preferredLanguage") || "vi";
  applyLanguage(savedLanguage);

  const languageToggle = document.querySelector("[data-language-toggle]");
  if (languageToggle) {

    languageToggle.addEventListener("click", () => {
      const nextLanguage = document.documentElement.lang === "vi" ? "en" : "vi";
      localStorage.setItem("preferredLanguage", nextLanguage);
      applyLanguage(nextLanguage);
    });
  }

});
