import { Star } from "lucide-react";

type StarRatingProps = {
  rating: number;
  max?: number;
  size?: "sm" | "md" | "lg";
  showValue?: boolean;
  className?: string;
};

const sizeMap = {
  sm: "size-3.5",
  md: "size-4",
  lg: "size-5",
};

export function StarRating({
  rating,
  max = 5,
  size = "md",
  showValue = false,
  className = "",
}: StarRatingProps) {
  const rounded = Math.round(rating);
  const iconSize = sizeMap[size];

  return (
    <span className={`inline-flex items-center gap-1 ${className}`}>
      <span className="inline-flex gap-0.5" aria-label={`${rating} / ${max}`}>
        {Array.from({ length: max }).map((_, i) => (
          <Star
            key={i}
            className={`${iconSize} ${i < rounded ? "fill-amber-400 text-amber-400" : "text-slate-300 dark:text-slate-600"}`}
          />
        ))}
      </span>
      {showValue && rating > 0 && (
        <span className="text-xs font-semibold text-slate-700 dark:text-slate-300">
          {rating.toFixed(1)}
        </span>
      )}
    </span>
  );
}

type InteractiveStarRatingProps = {
  value: number;
  onChange: (value: number) => void;
  size?: "sm" | "md" | "lg";
};

export function InteractiveStarRating({ value, onChange, size = "lg" }: InteractiveStarRatingProps) {
  const iconSize = sizeMap[size];
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map((n) => (
        <button
          key={n}
          type="button"
          onClick={() => onChange(n)}
          className="inline-flex min-h-11 min-w-11 items-center justify-center rounded-lg p-2"
          aria-label={`${n}`}
        >
          <Star
            className={`${iconSize} ${n <= value ? "fill-amber-400 text-amber-400" : "text-slate-300"}`}
          />
        </button>
      ))}
    </div>
  );
}
