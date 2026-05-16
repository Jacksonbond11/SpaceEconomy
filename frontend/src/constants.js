export const ZONES = [
  { id: "deep",   label: "GEO + Beyond",     altRange: "35,786 km +",    yPct: 0.33, kmLo: 35000, kmHi: 400000 },
  { id: "leo",    label: "LEO",              altRange: "450 – 2,000 km", yPct: 0.50, kmLo: 450,   kmHi: 2000 },
  { id: "vleo",   label: "VLEO",             altRange: "150 – 450 km",   yPct: 0.68, kmLo: 150,   kmHi: 450 },
  { id: "launch", label: "Surface / Launch", altRange: "0 – 100 km",     yPct: 0.88, kmLo: 0,     kmHi: 100 },
];

export const SECTORS = ["Launch", "Sat ops", "Imagery", "Defense", "Comms"];

export const SECTOR_COLORS = {
  "Launch":  "#fbbf77",
  "Sat ops": "#7dd3fc",
  "Imagery": "#a78bfa",
  "Defense": "#ff9aa2",
  "Comms":   "#66e3a4",
};
