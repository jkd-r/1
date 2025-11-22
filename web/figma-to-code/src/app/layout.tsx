import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Figma to Code Generator',
  description: 'Generate HTML, CSS, JavaScript, TypeScript, React, and Vue code from design inputs',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="bg-gray-50">{children}</body>
    </html>
  );
}
