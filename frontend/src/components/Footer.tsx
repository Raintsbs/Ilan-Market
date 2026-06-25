"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { ThemeLanguageControls } from "@/components/ThemeLanguageControls";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { siteShell } from "@/lib/uiStyles";
import type { PublicStaticPageListItem } from "@/lib/types";

const CONTACT_EMAIL = "tahatokay2006@gmail.com";

export function Footer() {
  const { t } = useLocale();
  const [pages, setPages] = useState<PublicStaticPageListItem[]>([]);

  useEffect(() => {
    api.getStaticPages().then((res) => {
      if (res.success && res.data) setPages(res.data);
    });
  }, []);

  return (
    <footer className="mt-auto border-t border-slate-800 bg-slate-950 text-slate-300">
      <div className={`${siteShell} py-10 sm:py-12`}>
        <div className="flex flex-col gap-10 lg:flex-row lg:items-start lg:justify-between">
          <div className="max-w-sm">
            <p className="text-lg font-bold text-white">
              İlan<span className="text-blue-400">Market</span>
            </p>
            <p className="mt-2 text-sm leading-relaxed text-slate-400">{t("footer.tagline")}</p>
            <p className="mt-6 text-xs text-slate-500">
              © {new Date().getFullYear()} {t("footer.copyright")}
            </p>
          </div>

          <div className="grid flex-1 gap-8 sm:grid-cols-2 lg:max-w-xl">
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                {t("footer.help")}
              </h3>
              <div className="mt-3 flex flex-col gap-2">
                <Link href="/yardim" className="text-sm text-slate-300 transition hover:text-white">
                  {t("footer.help")}
                </Link>
                <Link href="/blog" className="text-sm text-slate-300 transition hover:text-white">
                  {t("nav.blog")}
                </Link>
                {pages.map((p) => (
                  <Link
                    key={p.slug}
                    href={`/sayfa/${p.slug}`}
                    className="text-sm text-slate-300 transition hover:text-white"
                  >
                    {p.title}
                  </Link>
                ))}
              </div>
            </div>
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-500">
                {t("footer.contact")}
              </h3>
              <p className="mt-3 text-sm text-slate-400">{t("footer.contactEmail")}</p>
              <a
                href={`mailto:${CONTACT_EMAIL}`}
                className="mt-1 inline-block text-sm font-medium text-blue-400 transition hover:text-blue-300"
              >
                {CONTACT_EMAIL}
              </a>
            </div>
          </div>

          <div className="lg:pt-1">
            <ThemeLanguageControls onDarkBackground />
          </div>
        </div>
      </div>
    </footer>
  );
}
