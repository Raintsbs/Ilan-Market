const TR_MAP: Record<string, string> = {
  ç: "c",
  Ç: "c",
  ğ: "g",
  Ğ: "g",
  ı: "i",
  I: "i",
  İ: "i",
  ö: "o",
  Ö: "o",
  ş: "s",
  Ş: "s",
  ü: "u",
  Ü: "u",
};

export function toSlug(text: string): string {
  if (!text?.trim()) return "kategori";
  let s = text.trim();
  s = s
    .split("")
    .map((ch) => TR_MAP[ch] ?? ch)
    .join("");
  s = s.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
  s = s.toLowerCase().replace(/[^a-z0-9\s-/]/g, "");
  s = s.replace(/[\s/]+/g, "-").replace(/-+/g, "-").replace(/^-|-$/g, "");
  return s || "kategori";
}
