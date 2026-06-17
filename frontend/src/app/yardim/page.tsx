"use client";

import Link from "next/link";
import { useLocale } from "@/context/LocaleContext";
import { PageHeader } from "@/components/PageHeader";
import type { MessageKey } from "@/lib/i18n/messages";
import { alertInfo, linkBack, pageContainerMd, surfaceCard } from "@/lib/uiStyles";

const FAQ_ITEMS: { q: MessageKey; a: MessageKey }[] = [
  { q: "help.q1", a: "help.a1" },
  { q: "help.q2", a: "help.a2" },
  { q: "help.q3", a: "help.a3" },
  { q: "help.q4", a: "help.a4" },
  { q: "help.q5", a: "help.a5" },
];

export default function HelpPage() {
  const { t } = useLocale();

  return (
    <div className={pageContainerMd}>
      <Link href="/" className={linkBack}>
        ← {t("nav.ads")}
      </Link>
      <PageHeader title={t("help.title")} subtitle={t("help.intro")} className="mt-4" />

      <div className="mt-8 space-y-4">
        {FAQ_ITEMS.map((item) => (
          <details key={item.q} className={`group ${surfaceCard}`}>
            <summary className="cursor-pointer list-none px-5 py-4 font-semibold text-slate-900 marker:content-none dark:text-slate-100">
              {t(item.q)}
            </summary>
            <p className="border-t border-slate-100 px-5 py-4 text-sm text-slate-600 dark:border-slate-800 dark:text-slate-400">
              {t(item.a)}
            </p>
          </details>
        ))}
      </div>

      <div className={`mt-10 p-6 ${alertInfo}`}>
        <h2 className="font-semibold text-slate-900 dark:text-white">{t("contact.title")}</h2>
        <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{t("contact.desc")}</p>
        <a
          href="mailto:tahatokay2006@gmail.com"
          className="mt-2 inline-block font-medium text-blue-600 hover:underline dark:text-blue-400"
        >
          tahatokay2006@gmail.com
        </a>
      </div>
    </div>
  );
}
