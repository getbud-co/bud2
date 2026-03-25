import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Bud",
  description: "Bud - Goals and Indicators Management",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="pt-BR">
      <body>{children}</body>
    </html>
  );
}
