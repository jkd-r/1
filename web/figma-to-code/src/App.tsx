import { useMemo } from "react";
import { useTheme } from "./components/theme-provider";
import { cn } from "./lib/utils";

function App() {
  const { theme, resolvedTheme, setTheme } = useTheme();

  const headline = useMemo(
    () =>
      theme === "system"
        ? "System aware design system scaffold"
        : `Currently previewing the ${resolvedTheme} theme`,
    [theme, resolvedTheme]
  );

  const handleCycleTheme = () => {
    setTheme(resolvedTheme === "dark" ? "light" : "dark");
  };

  return (
    <div className="min-h-screen bg-background text-foreground antialiased">
      <div className="container flex min-h-screen flex-col items-center justify-center gap-10 py-16 text-center">
        <span className="text-xs uppercase tracking-[0.3em] text-muted-foreground">
          figma → code workspace
        </span>
        <div className="space-y-6">
          <h1 className="text-balance text-4xl font-semibold leading-tight sm:text-5xl">
            {headline}
          </h1>
          <p className="text-balance text-base text-muted-foreground sm:text-lg">
            This React + Vite + Tailwind shell will host the generated UI while
            the rest of the integration is built out. Theme tokens and routing
            ready to plug into your design system.
          </p>
        </div>
        <div className="flex flex-wrap items-center justify-center gap-3">
          {(["light", "dark", "system"] as const).map((option) => (
            <button
              key={option}
              type="button"
              onClick={() => setTheme(option)}
              className={cn(
                "rounded-full border px-5 py-2 text-sm font-medium transition",
                option === theme
                  ? "border-primary bg-primary text-primary-foreground"
                  : "border-border bg-card text-muted-foreground hover:text-foreground"
              )}
            >
              {option}
            </button>
          ))}
          <button
            type="button"
            onClick={handleCycleTheme}
            className="rounded-full border border-dashed border-input px-5 py-2 text-sm font-semibold text-foreground transition hover:bg-muted"
          >
            Toggle {resolvedTheme === "dark" ? "Light" : "Dark"}
          </button>
        </div>
        <div className="rounded-3xl border border-dashed border-border/70 bg-card/50 p-6 text-left shadow-sm">
          <p className="text-sm text-muted-foreground">
            Next up: wire Figma nodes to components, add routing, and hydrate real
            data sources. This scaffold keeps Unity assets untouched while giving
            web contributors a modern DX.
          </p>
        </div>
      </div>
    </div>
  );
}

export default App;
