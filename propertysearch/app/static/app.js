// Dashboard behaviour: lanes, filters, shortlist, and the map.

const state = {
  lane: "move_in",
  municipality: "",
  maxPrice: "",
  minSurface: "",
  minBedrooms: "",
  epc: "",
  fits: false,
};

let map = null;
let markers = null;

const cardsEl = document.getElementById("cards");
const emptyEl = document.getElementById("empty");
const bannerEl = document.getElementById("dataset-banner");
const disclaimerEl = document.getElementById("disclaimer");
const runLogEl = document.getElementById("run-log");
const municipalityEl = document.getElementById("filter-municipality");

document.querySelectorAll(".lane").forEach((button) => {
  button.addEventListener("click", () => {
    state.lane = button.dataset.lane;
    document.querySelectorAll(".lane").forEach((item) => {
      item.classList.toggle("is-active", item === button);
    });
    load();
  });
});

document.getElementById("filters").addEventListener("change", () => {
  state.municipality = municipalityEl.value;
  state.maxPrice = document.getElementById("filter-max-price").value;
  state.minSurface = document.getElementById("filter-min-surface").value;
  state.minBedrooms = document.getElementById("filter-min-bedrooms").value;
  state.epc = document.getElementById("filter-epc").value;
  state.fits = document.getElementById("filter-fits").checked;
  load();
});

document.getElementById("filter-max-price").addEventListener("input", () => {
  state.maxPrice = document.getElementById("filter-max-price").value;
  load();
});

load();

async function load() {
  const params = new URLSearchParams();
  params.set("lane", state.lane);
  if (state.municipality) params.set("municipality", state.municipality);
  if (state.maxPrice) params.set("max_price", state.maxPrice);
  if (state.minSurface) params.set("min_surface", state.minSurface);
  if (state.minBedrooms) params.set("min_bedrooms", state.minBedrooms);
  if (state.epc) params.set("epc", state.epc);
  if (state.fits) params.set("fits", "1");
  const response = await fetch("/api/listings?" + params.toString());
  const data = await response.json();
  render(data);
}

function render(data) {
  disclaimerEl.textContent = data.disclaimer || "";
  bannerEl.hidden = !data.showing_samples;
  bannerEl.textContent = data.showing_samples
    ? "Showing sample fixtures, not live listings. They are labeled Sample on every card. Run python run.py --ingest to try Immoweb, Zimmo, and Immovlan."
    : "";
  document.querySelectorAll("[data-count]").forEach((node) => {
    node.textContent = String(data.counts[node.dataset.count] ?? 0);
  });
  fillMunicipalities(data.municipalities || []);
  renderRun(data.last_run);
  cardsEl.replaceChildren();
  emptyEl.hidden = data.listings.length > 0;
  data.listings.forEach((listing) => cardsEl.appendChild(cardView(listing)));
  syncMap(data.listings);
}

function fillMunicipalities(names) {
  const current = state.municipality;
  const options = [new Option("All", "")];
  names.forEach((name) => options.push(new Option(name, name)));
  municipalityEl.replaceChildren(...options);
  municipalityEl.value = names.includes(current) ? current : "";
}

function cardView(listing) {
  const card = document.createElement("article");
  card.className = "card" + (listing.lane === "review" ? " review" : "");
  card.dataset.testid = "card";
  card.id = "listing-" + listing.id;

  const photos = document.createElement("div");
  photos.className = "photos";
  const photoList = listing.lane === "review" ? listing.photos : listing.photos.slice(0, 1);
  if (photoList.length === 0) {
    const placeholder = document.createElement("div");
    placeholder.className = "photos";
    placeholder.textContent = "No photo stored";
    photos.appendChild(placeholder);
  } else {
    photoList.forEach((url) => {
      const image = document.createElement("img");
      image.src = url;
      image.alt = listing.is_sample ? "Sample photo" : "Listing photo";
      photos.appendChild(image);
    });
  }

  const body = document.createElement("div");
  const badges = document.createElement("div");
  badges.className = "badges";
  if (listing.is_sample) badges.appendChild(badge("Sample", "sample"));
  if (listing.over_loan) badges.appendChild(badge("Over the loan", "over"));
  else if (listing.fits_loan) badges.appendChild(badge("Fits €246,000", "fit"));
  else if (!listing.renovation_unknown && listing.cash_needed_high > 0) {
    badges.appendChild(badge("Over at the high renovation estimate", "over"));
  }

  const title = document.createElement("h2");
  title.textContent = listing.title || "Apartment";
  const price = document.createElement("p");
  price.className = "price";
  price.dataset.testid = "card-price";
  price.textContent = euro(listing.price);
  const place = document.createElement("p");
  place.dataset.testid = "card-place";
  place.textContent = [listing.street, listing.place].filter(Boolean).join(", ");

  const facts = document.createElement("ul");
  facts.className = "facts";
  [
    listing.surface_m2 ? `${listing.surface_m2} m²` : null,
    listing.bedrooms ? `${listing.bedrooms} bedrooms` : null,
    listing.epc_label ? `EPC ${listing.epc_label}` : "EPC unknown",
    listing.building_condition ? conditionLabel(listing.building_condition) : "Condition unknown",
  ].filter(Boolean).forEach((text) => {
    const item = document.createElement("li");
    item.textContent = text;
    facts.appendChild(item);
  });

  const money = document.createElement("p");
  money.className = "money";
  money.textContent = moneyLine(listing);
  const reason = document.createElement("p");
  reason.className = "reason";
  reason.textContent = listing.reason || "";
  const support = document.createElement("p");
  support.className = "support";
  support.textContent = listing.supporting_note || "";

  const sources = document.createElement("ul");
  sources.className = "sources";
  listing.sources.forEach((source) => {
    const item = document.createElement("li");
    const link = document.createElement("a");
    link.href = source.url;
    link.dataset.testid = "source-link";
    link.textContent = sourceLabel(source);
    if (String(source.url).startsWith("http")) {
      link.target = "_blank";
      link.rel = "noopener noreferrer";
    }
    item.appendChild(link);
    sources.appendChild(item);
  });

  const toggle = document.createElement("button");
  toggle.type = "button";
  toggle.className = "shortlist" + (listing.shortlisted ? " is-on" : "");
  toggle.dataset.testid = "shortlist-toggle";
  toggle.setAttribute("aria-pressed", listing.shortlisted ? "true" : "false");
  toggle.textContent = listing.shortlisted ? "On the shortlist" : "Add to shortlist";
  toggle.addEventListener("click", async () => {
    await fetch("/api/listings/" + listing.id + "/shortlist", { method: "POST" });
    load();
  });

  body.append(badges, title, price, place, facts, money, reason, support, sources, toggle);
  card.append(photos, body);
  return card;
}

function moneyLine(listing) {
  const costs = `Purchase costs ${euro(listing.purchase_costs)} (2% duty ${euro(listing.registration_duty)} + notary ${euro(listing.notary_and_admin)}).`;
  const reno = renovationText(listing);
  const cash = cashText(listing);
  const head = headroomText(listing);
  return `${costs} Renovation ${reno}. ${head} Cash still needed after the €246,000 loan: ${cash}.`;
}

function renovationText(listing) {
  if (listing.renovation_unknown || listing.reno_low === null) return "unknown";
  if (listing.band === "to_restore") return `${euro(listing.reno_low)} and up`;
  return `${euro(listing.reno_low)}–${euro(listing.reno_high)} (${listing.band_label})`;
}

function cashText(listing) {
  if (listing.cash_needed_low === null) return "unknown until the photos are reviewed";
  if (listing.band === "to_restore") return `${euro(listing.cash_needed_low)} and up`;
  if (listing.cash_needed_low === listing.cash_needed_high) return euro(listing.cash_needed_low);
  return `${euro(listing.cash_needed_low)}–${euro(listing.cash_needed_high)}`;
}

function headroomText(listing) {
  if (listing.headroom_high === null) return "Loan headroom unknown.";
  if (listing.over_loan) return "Over the loan even at the low renovation estimate.";
  if (listing.headroom_low < 0) return "Inside the loan only at the low end of the renovation range.";
  return `Loan headroom ${euro(listing.headroom_low)}–${euro(listing.headroom_high)}.`;
}

function sourceLabel(source) {
  const name = source.portal.charAt(0).toUpperCase() + source.portal.slice(1);
  if (source.days_on_market) return `${name} · ${source.days_on_market} days on market`;
  return name;
}

function conditionLabel(value) {
  return String(value).replaceAll("_", " ").toLowerCase();
}

function badge(text, kind) {
  const node = document.createElement("span");
  node.className = "badge " + kind;
  node.textContent = text;
  return node;
}

function euro(value) {
  if (value === null || value === undefined) return "—";
  return new Intl.NumberFormat("en-GB", {
    style: "currency",
    currency: "EUR",
    maximumFractionDigits: 0,
  }).format(value);
}

function renderRun(run) {
  runLogEl.replaceChildren();
  if (!run) {
    runLogEl.textContent = "No ingest recorded yet.";
    return;
  }
  const intro = document.createElement("p");
  intro.textContent = `Live listings ${run.live_listings ?? 0}, samples ${run.sample_listings ?? 0}. Later sources not fetched: ${(run.later_portals_not_fetched || []).join(", ") || "none"}.`;
  const list = document.createElement("ul");
  (run.sources || []).forEach((source) => {
    const item = document.createElement("li");
    item.textContent = `${source.portal}: ${source.status} — ${source.detail}`;
    list.appendChild(item);
  });
  if (!run.sources || run.sources.length === 0) {
    const item = document.createElement("li");
    item.textContent = "This snapshot was opened without a portal fetch.";
    list.appendChild(item);
  }
  runLogEl.append(intro, list);
}

function syncMap(listings) {
  const points = listings.filter((listing) => listing.latitude && listing.longitude);
  const wrap = document.getElementById("map-wrap");
  if (!points.length || typeof L === "undefined") {
    wrap.hidden = points.length === 0;
    return;
  }
  wrap.hidden = false;
  if (!map) {
    map = L.map("map", { scrollWheelZoom: false });
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      maxZoom: 19,
      attribution: "&copy; OpenStreetMap",
    }).addTo(map);
    markers = L.layerGroup().addTo(map);
  }
  markers.clearLayers();
  const bounds = [];
  points.forEach((listing) => {
    const marker = L.circleMarker([listing.latitude, listing.longitude], {
      radius: 8,
      color: listing.lane === "renovate" ? "#8a3b12" : listing.lane === "review" ? "#243044" : "#1f4d3a",
      weight: 2,
      fillOpacity: 0.85,
    });
    marker.bindPopup(`<strong>${escapeHtml(euro(listing.price))}</strong><br>${escapeHtml(listing.place)}`);
    marker.addTo(markers);
    bounds.push([listing.latitude, listing.longitude]);
  });
  if (bounds.length === 1) map.setView(bounds[0], 12);
  else map.fitBounds(bounds, { padding: [24, 24] });
  setTimeout(() => map.invalidateSize(), 50);
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");
}
