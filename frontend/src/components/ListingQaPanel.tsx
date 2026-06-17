"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formFieldClass } from "@/lib/formStyles";
import { formatDate } from "@/lib/status";
import { surfaceCard } from "@/lib/uiStyles";
import type { ListingQuestion } from "@/lib/types";

type Props = {
  advertisementId: number;
  isOwner: boolean;
};

export function ListingQaPanel({ advertisementId, isOwner }: Props) {
  const { t, locale } = useLocale();
  const { isAuthenticated } = useAuth();
  const { showToast } = useToast();
  const [items, setItems] = useState<ListingQuestion[]>([]);
  const [question, setQuestion] = useState("");
  const [answerDrafts, setAnswerDrafts] = useState<Record<number, string>>({});
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    const res = await api.getListingQuestions(advertisementId);
    if (res.success && res.data) setItems(res.data);
    setLoading(false);
  }, [advertisementId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function ask(e: React.FormEvent) {
    e.preventDefault();
    if (!question.trim()) return;
    try {
      const res = await api.askListingQuestion(advertisementId, question.trim());
      if (res.success) {
        setQuestion("");
        await load();
        showToast(t("qa.sent"), "success");
      } else showToast(res.message || t("qa.failed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("qa.failed"), "error");
    }
  }

  async function answer(questionId: number) {
    const text = answerDrafts[questionId]?.trim();
    if (!text) return;
    try {
      const res = await api.answerListingQuestion(questionId, text);
      if (res.success) {
        await load();
        showToast(t("qa.answered"), "success");
      }
    } catch {
      showToast(t("qa.failed"), "error");
    }
  }

  return (
    <section id="sorular" className={`scroll-mt-24 p-6 ${surfaceCard}`}>
      <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t("qa.title")}</h2>

      {!loading && items.length === 0 && (
        <p className="mt-2 text-sm text-slate-500">{t("qa.empty")}</p>
      )}

      <ul className="mt-4 space-y-4">
        {items.map((q) => (
          <li key={q.id} className="rounded-xl border border-slate-100 p-4 dark:border-slate-800">
            <p className="text-sm font-medium text-slate-900 dark:text-white">{q.userName}</p>
            <p className="mt-1 text-sm text-slate-700 dark:text-slate-300">{q.question}</p>
            <p className="mt-1 text-xs text-slate-500">{formatDate(q.createdTime, locale)}</p>
            {q.answer ? (
              <div className="mt-3 rounded-lg bg-emerald-50 p-3 text-sm dark:bg-emerald-950/30">
                <p className="font-medium text-emerald-800 dark:text-emerald-300">{t("qa.answer")}</p>
                <p className="mt-1 text-slate-700 dark:text-slate-300">{q.answer}</p>
              </div>
            ) : isOwner ? (
              <div className="mt-3 flex gap-2">
                <input
                  value={answerDrafts[q.id] ?? ""}
                  onChange={(e) => setAnswerDrafts((d) => ({ ...d, [q.id]: e.target.value }))}
                  placeholder={t("qa.answerPlaceholder")}
                  className={`flex-1 ${formFieldClass}`}
                />
                <button
                  type="button"
                  onClick={() => answer(q.id)}
                  className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white"
                >
                  {t("qa.reply")}
                </button>
              </div>
            ) : (
              <p className="mt-2 text-xs text-slate-500">{t("qa.awaiting")}</p>
            )}
          </li>
        ))}
      </ul>

      {isAuthenticated && !isOwner && (
        <form onSubmit={ask} className="mt-6 border-t border-slate-100 pt-6 dark:border-slate-800">
          <textarea
            value={question}
            onChange={(e) => setQuestion(e.target.value)}
            placeholder={t("qa.askPlaceholder")}
            rows={3}
            className={formFieldClass}
          />
          <button type="submit" className="mt-3 rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white">
            {t("qa.ask")}
          </button>
        </form>
      )}
    </section>
  );
}
