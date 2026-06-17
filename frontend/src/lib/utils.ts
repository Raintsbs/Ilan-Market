/** shadcn/ui uyumlu sınıf birleştirici */
export function cn(...classes: (string | false | null | undefined)[]) {
  return classes.filter(Boolean).join(" ");
}
