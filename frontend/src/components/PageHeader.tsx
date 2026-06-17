import { pageSubtitle, pageTitle } from "@/lib/uiStyles";

type PageHeaderProps = {
  title: string;
  subtitle?: string;
  className?: string;
};

export function PageHeader({ title, subtitle, className = "" }: PageHeaderProps) {
  return (
    <header className={className}>
      <h1 className={pageTitle}>{title}</h1>
      {subtitle ? <p className={pageSubtitle}>{subtitle}</p> : null}
    </header>
  );
}
