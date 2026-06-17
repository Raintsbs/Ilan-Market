"use client";

import React, { useEffect, useMemo, useState } from "react";

type CardState = {
  number: string;
  holder: string;
  month: string;
  year: string;
  cvv: string;
};

type CardValidity = {
  number: boolean;
  holder: boolean;
  month: boolean;
  year: boolean;
  cvv: boolean;
  allValid: boolean;
};

type CreditCardLabels = {
  cardBrand?: string;
  cardHolder?: string;
  expires?: string;
  cvvBack?: string;
  number?: string;
  holder?: string;
  expiration?: string;
  cvv?: string;
  month?: string;
  year?: string;
  submit?: string;
  submitDisabled?: string;
  numberInvalid?: string;
};

type Props = {
  defaultNumber?: string;
  defaultHolder?: string;
  defaultMonth?: string;
  defaultYear?: string;
  defaultCVV?: string;
  maskMiddle?: boolean;
  ring1?: string;
  ring2?: string;
  showSubmit?: boolean;
  submitting?: boolean;
  labels?: CreditCardLabels;
  onChange?: (state: CardState, validity: CardValidity) => void;
  onSubmit?: (state: CardState, validity: CardValidity) => void;
  className?: string;
};

function formatNumberSpaces(num: string): string {
  return num.replace(/\s+/g, "").replace(/(\d{4})(?=\d)/g, "$1 ");
}

function clampDigits(value: string, maxLen: number) {
  return value.replace(/\D/g, "").slice(0, maxLen);
}

const defaultLabels: CreditCardLabels = {
  cardBrand: "CreditCard",
  cardHolder: "Card Holder",
  expires: "Expires",
  cvvBack: "CVV",
  number: "Card Number",
  holder: "Card Holder",
  expiration: "Expiration Date",
  cvv: "CVV",
  month: "Month",
  year: "Year",
  submit: "Submit",
  submitDisabled: "Complete all fields",
  numberInvalid: "Card number looks invalid",
};

const CreditCardForm = ({
  defaultNumber = "",
  defaultHolder = "",
  defaultMonth = "",
  defaultYear = "",
  defaultCVV = "",
  maskMiddle = true,
  ring1 = "#3b82f6",
  ring2 = "#6366f1",
  showSubmit = true,
  submitting = false,
  labels: labelOverrides,
  onChange,
  onSubmit,
  className = "",
}: Props) => {
  const labels = { ...defaultLabels, ...labelOverrides };
  const [number, setNumber] = useState(clampDigits(defaultNumber, 16));
  const [holder, setHolder] = useState(defaultHolder.toUpperCase());
  const [month, setMonth] = useState(defaultMonth);
  const [year, setYear] = useState(defaultYear);
  const [cvv, setCVV] = useState(clampDigits(defaultCVV, 3));
  const [focusField, setFocusField] = useState<null | "number" | "holder" | "expire" | "cvv">(null);

  const flip = focusField === "cvv";

  const monthValid = useMemo(() => {
    if (!month) return false;
    const n = Number(month);
    return n >= 1 && n <= 12;
  }, [month]);

  const yearValid = useMemo(() => {
    if (!year) return false;
    const n = Number(year);
    const fullYear = year.length === 2 ? 2000 + n : n;
    return fullYear >= new Date().getFullYear();
  }, [year]);

  const validity: CardValidity = useMemo(() => {
    const numberValid = number.length === 16;
    const holderValid = holder.trim().length >= 2;
    const cvvValid = /^\d{3}$/.test(cvv);
    return {
      number: numberValid,
      holder: holderValid,
      month: monthValid,
      year: yearValid,
      cvv: cvvValid,
      allValid: numberValid && holderValid && monthValid && yearValid && cvvValid,
    };
  }, [number, holder, monthValid, yearValid, cvv]);

  useEffect(() => {
    onChange?.({ number, holder, month, year, cvv }, validity);
  }, [number, holder, month, year, cvv, validity, onChange]);

  const displayDigits = useMemo(() => number.slice(0, 16).split(""), [number]);

  const displayedSlots = useMemo(() => {
    const arr: { textTop: string; filed: boolean }[] = [];
    for (let i = 0; i < 16; i++) {
      let content = "#";
      if (i < displayDigits.length) {
        const d = displayDigits[i];
        const shouldMask = maskMiddle && i >= 4 && i <= 11;
        content = shouldMask ? "*" : d;
      }
      arr.push({ textTop: content, filed: i < displayDigits.length });
    }
    return arr;
  }, [displayDigits, maskMiddle]);

  const highlightClass = (() => {
    switch (focusField) {
      case "number":
        return "highlight__number";
      case "holder":
        return "highlight__holder";
      default:
        return "hidden";
    }
  })();

  const expiryDisplay = `${monthValid ? month.padStart(2, "0") : "MM"}/${
    yearValid ? (year.length >= 4 ? year.slice(-2) : year.padStart(2, "0")) : "YY"
  }`;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validity.allValid || submitting) return;
    onSubmit?.({ number, holder, month, year, cvv }, validity);
  };

  return (
    <section className={`ccp ${className}`}>
      <div className="wrap">
        <section id="card" className={`card ${flip ? "flip" : ""}`}>
          <div id="highlight" className={highlightClass} />

          <section className="card__front" style={{ ["--ring1" as string]: ring1, ["--ring2" as string]: ring2 }}>
            <div className="card__header">
              <div>{labels.cardBrand}</div>
              <svg xmlns="http://www.w3.org/2000/svg" height="40" width="60" viewBox="-96 -98.908 832 593.448">
                <path fill="#ff5f00" d="M224.833 42.298h190.416v311.005H224.833z" />
                <path
                  d="M244.446 197.828a197.448 197.448 0 0175.54-155.475 197.777 197.777 0 100 311.004 197.448 197.448 0 01-75.54-155.53z"
                  fill="#eb001b"
                />
                <path
                  d="M621.101 320.394v-6.372h2.747v-1.319h-6.537v1.319h2.582v6.373zm12.691 0v-7.69h-1.978l-2.307 5.493-2.308-5.494h-1.977v7.691h1.428v-5.823l2.143 5h1.483l2.143-5v5.823z"
                  fill="#f79e1b"
                />
                <path
                  d="M640 197.828a197.777 197.777 0 01-320.015 155.474 197.777 197.777 0 000-311.004A197.777 197.777 0 01640 197.773z"
                  fill="#f79e1b"
                />
              </svg>
            </div>

            <div id="card_number" className="card__number" aria-label={labels.number}>
              {displayedSlots.map((slot, idx) => (
                <span key={idx} className="slot">
                  <span className={`digit ${slot.filed ? "filed" : ""}`}>
                    <span className="row placeholder">#</span>
                    <span className="row value">{slot.textTop}</span>
                  </span>
                </span>
              ))}
            </div>

            <div className="card__footer">
              <div className="card__holder">
                <div className="card__section__title">{labels.cardHolder}</div>
                <div id="card_holder">{holder || "NAME ON CARD"}</div>
              </div>
              <div className="card__expires">
                <div className="card__section__title">{labels.expires}</div>
                <div id="card_expires_value">{expiryDisplay}</div>
              </div>
            </div>
          </section>

          <section className="card__back" style={{ ["--ring1" as string]: ring1, ["--ring2" as string]: ring2 }}>
            <div className="card__hide_line" />
            <div className="card_cvv">
              <span>{labels.cvvBack}</span>
              <div id="card_cvv_field" className="card_cvv_field">
                {cvv || "•••"}
              </div>
            </div>
          </section>
        </section>

        <form className="form" onSubmit={handleSubmit} noValidate>
          <div>
            <label htmlFor="number">{labels.number}</label>
            <input
              id="number"
              inputMode="numeric"
              autoComplete="cc-number"
              placeholder="1234 5678 9012 3456"
              maxLength={19}
              value={formatNumberSpaces(number)}
              onChange={(e) => setNumber(clampDigits(e.target.value, 16))}
              onFocus={() => setFocusField("number")}
              onBlur={() => setFocusField(null)}
              aria-invalid={!validity.number}
            />
            {!validity.number && number.length === 16 && (
              <small className="err">{labels.numberInvalid}</small>
            )}
          </div>

          <div>
            <label htmlFor="holder">{labels.holder}</label>
            <input
              id="holder"
              type="text"
              autoComplete="cc-name"
              placeholder="JANE DOE"
              value={holder}
              onChange={(e) => setHolder(e.target.value.toUpperCase())}
              onFocus={() => setFocusField("holder")}
              onBlur={() => setFocusField(null)}
              aria-invalid={!validity.holder}
            />
          </div>

          <div className="filed__row">
            <div className="filed__cell">
              <label htmlFor="expiration_month">{labels.month}</label>
              <input
                id="expiration_month"
                inputMode="numeric"
                autoComplete="cc-exp-month"
                placeholder="AA"
                maxLength={2}
                value={month}
                onChange={(e) => {
                  const v = clampDigits(e.target.value, 2);
                  if (v === "" || Number(v) <= 12) setMonth(v);
                }}
                onFocus={() => setFocusField("expire")}
                onBlur={() => {
                  if (month.length === 1) setMonth(month.padStart(2, "0"));
                  setFocusField((f) => (f === "expire" ? null : f));
                }}
                aria-invalid={!validity.month}
              />
            </div>

            <div className="filed__cell filed__cell--year">
              <label htmlFor="expiration_year">{labels.year}</label>
              <input
                id="expiration_year"
                inputMode="numeric"
                autoComplete="cc-exp-year"
                placeholder="YYYY"
                maxLength={4}
                value={year}
                onChange={(e) => setYear(clampDigits(e.target.value, 4))}
                onFocus={() => setFocusField("expire")}
                onBlur={() => setFocusField((f) => (f === "expire" ? null : f))}
                aria-invalid={!validity.year}
              />
            </div>

            <div className="filed__cell filed__cell--cvv">
              <label htmlFor="cvv">{labels.cvv}</label>
              <input
                id="cvv"
                inputMode="numeric"
                autoComplete="cc-csc"
                placeholder="•••"
                maxLength={3}
                value={cvv}
                onChange={(e) => setCVV(clampDigits(e.target.value, 3))}
                onFocus={() => setFocusField("cvv")}
                onBlur={() => setFocusField(null)}
                aria-invalid={!validity.cvv}
              />
            </div>
          </div>

          {showSubmit && (
            <button
              className="submit"
              type="submit"
              disabled={!validity.allValid || submitting}
              aria-disabled={!validity.allValid || submitting}
              style={{ opacity: validity.allValid && !submitting ? 1 : 0.6 }}
            >
              {submitting ? "…" : validity.allValid ? labels.submit : labels.submitDisabled}
            </button>
          )}
        </form>
      </div>

      <style jsx>{`
        .ccp {
          width: 100%;
          display: flex;
          justify-content: center;
          padding: 0;
          color: var(--foreground);
        }
        .wrap {
          width: 100%;
          max-width: 980px;
          display: grid;
          grid-template-columns: minmax(280px, 1fr) minmax(320px, 1.1fr);
          gap: 28px;
          align-items: start;
        }
        @media (max-width: 920px) {
          .wrap {
            grid-template-columns: 1fr;
          }
        }
        * {
          box-sizing: border-box;
        }
        #highlight {
          position: absolute;
          border: 1px solid #fff;
          border-radius: 12px;
          z-index: 1;
          width: 0;
          height: 0;
          top: 0;
          left: 0;
          box-shadow: 0 0 5px #fff;
          transition: 0.3s;
        }
        #highlight.highlight__number {
          width: 346px;
          height: 40px;
          top: 92px;
          left: 18px;
        }
        #highlight.highlight__holder {
          width: 264px;
          height: 56px;
          top: 156px;
          left: 18px;
        }
        #highlight.hidden {
          display: none;
        }
        .card {
          position: relative;
          width: 100%;
          max-width: 420px;
          margin: 0 auto;
          transform-style: preserve-3d;
          transition: 0.8s;
          perspective: 1000px;
        }
        .card.flip {
          transform: rotateY(180deg);
        }
        .card__front,
        .card__back {
          width: 100%;
          max-width: 420px;
          height: 233px;
          border-radius: 20px;
          padding: 24px 30px 30px;
          background: linear-gradient(to right bottom, #323941, #061018);
          box-shadow: 0 33px 50px -15px rgba(50, 55, 63, 0.66);
          color: #fff;
          overflow: hidden;
          margin: 0 auto;
          backface-visibility: hidden;
          position: relative;
        }
        @media (max-width: 450px) {
          .card__front,
          .card__back {
            padding: 12px 14px 16px;
            height: 206px;
          }
          #highlight.highlight__number {
            width: 300px;
            left: 14px;
          }
          #highlight.highlight__holder {
            width: 220px;
            left: 14px;
          }
        }
        .card__back {
          position: absolute;
          top: 0;
          left: 0;
          transform: rotateY(180deg);
          padding: 24px 0 0;
        }
        .card__front::before,
        .card__back::before {
          content: "";
          position: absolute;
          border: 16px solid var(--ring1, ${ring1});
          border-radius: 100%;
          left: -17%;
          top: -45px;
          height: 300px;
          width: 300px;
          filter: blur(13px);
        }
        .card__front::after,
        .card__back::after {
          content: "";
          position: absolute;
          border: 16px solid var(--ring2, ${ring2});
          border-radius: 100%;
          width: 300px;
          top: 55%;
          left: -200px;
          height: 300px;
          filter: blur(13px);
        }
        .card__hide_line {
          height: 40px;
          width: 100%;
          background-color: #6b7280;
          position: relative;
          z-index: 1;
        }
        .card_cvv {
          position: relative;
          z-index: 1;
          margin-top: 24px;
          padding: 0 32px;
          display: flex;
          flex-direction: column;
          align-items: end;
          font-size: 14px;
          font-weight: 600;
          text-transform: uppercase;
        }
        .card_cvv_field {
          margin-top: 6px;
          background-color: #fff;
          border-radius: 12px;
          height: 44px;
          width: 100%;
          color: #000;
          display: flex;
          align-items: center;
          justify-content: end;
          padding: 0 12px;
          font-size: 25px;
          line-height: 21px;
        }
        .card__header {
          display: flex;
          align-items: center;
          justify-content: space-between;
          font-weight: 600;
          margin-bottom: 32px;
          position: relative;
          z-index: 1;
        }
        .card__number {
          font-size: 22px;
          margin-bottom: 32px;
          position: relative;
          z-index: 1;
          display: flex;
          height: 33px;
          overflow: hidden;
          color: #fff;
        }
        .card__number .slot {
          display: inline-flex;
          margin-right: 0;
        }
        .card__number .slot:nth-child(4n) {
          margin-right: 10px;
        }
        .card__number .digit {
          display: flex;
          flex-direction: column;
          height: 33px;
          line-height: 33px;
          transition: transform 0.2s;
        }
        .card__number .digit.filed {
          transform: translateY(-33px);
        }
        .card__number .row {
          height: 33px;
          display: block;
        }
        .card__footer {
          display: flex;
          align-items: center;
          justify-content: space-between;
          position: relative;
          z-index: 1;
        }
        .card__holder {
          text-transform: uppercase;
        }
        .card__expires {
          text-align: right;
          min-width: 72px;
        }
        #card_expires_value {
          font-size: 16px;
          letter-spacing: 0.04em;
        }
        .card__section__title {
          font-size: 14px;
          font-weight: 600;
          text-transform: uppercase;
        }
        .form {
          border-radius: 16px;
          background: var(--card);
          width: 100%;
          max-width: none;
          margin: 0;
          padding: 28px;
          border: 1px solid var(--surface-border, #e2e8f0);
          box-shadow: 0 8px 32px rgba(15, 23, 42, 0.08);
          display: grid;
          gap: 16px;
          color: var(--card-foreground);
          color-scheme: light;
        }
        :global(.dark) .form {
          box-shadow: 0 8px 32px rgba(0, 0, 0, 0.35);
        }
        label {
          display: block;
          margin: 0 0 8px;
          font-weight: 500;
          font-size: 14px;
          line-height: 1.3;
          white-space: nowrap;
        }
        input {
          height: 58px;
          display: block;
          width: 100%;
          max-width: 100%;
          min-width: 0;
          border: 1px solid #cbd5e1;
          padding: 0 14px;
          transition: border-color 200ms ease, box-shadow 200ms ease;
          border-radius: 12px;
          outline: none;
          background-color: #ffffff;
          color: #0f172a;
          font-size: 18px;
          line-height: 58px;
        }
        input::placeholder {
          color: #94a3b8;
        }
        input:focus {
          border: 1px solid #2563eb;
          box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
        }
        .filed__row {
          display: grid;
          grid-template-columns: minmax(0, 0.85fr) minmax(0, 1.25fr) minmax(0, 1fr);
          gap: 14px;
          width: 100%;
        }
        .filed__cell {
          min-width: 0;
        }
        #expiration_month,
        #expiration_year,
        #cvv {
          text-align: center;
          font-variant-numeric: tabular-nums;
          letter-spacing: 0.04em;
          padding: 0 10px;
        }
        #expiration_year {
          letter-spacing: 0.06em;
        }
        @media (max-width: 520px) {
          .filed__row {
            grid-template-columns: 1fr 1fr;
          }
          .filed__cell--cvv {
            grid-column: 1 / -1;
          }
        }
        .err {
          color: #b42318;
          font-size: 12px;
          margin-top: 4px;
        }
        .submit {
          margin-top: 8px;
          height: 48px;
          border: none;
          border-radius: 10px;
          background: var(--primary, #2563eb);
          color: #fff;
          font-weight: 600;
          cursor: pointer;
        }
        .submit:disabled {
          cursor: not-allowed;
        }
      `}</style>
    </section>
  );
};

export { CreditCardForm };
export type { CardState, CardValidity, CreditCardLabels };
