import { Language, LANGUAGES } from '@/data/languages';
import { Framework, FRAMEWORK_TEMPLATES } from '@/data/frameworks';
import { DesignPreset, DesignInput, DESIGN_PRESETS } from '@/data/presets';

export interface GeneratedCode {
  html: string;
  css: string;
  js: string;
  ts?: string;
  jsx?: string;
  tsx?: string;
  vue?: string;
}

export interface CodeFile {
  name: string;
  language: string;
  content: string;
}

export class CodeGenerator {
  private preset: DesignPreset;
  private designInput: DesignInput;

  constructor(designInput: DesignInput) {
    this.designInput = designInput;
    this.preset = DESIGN_PRESETS[designInput.preset];
  }

  private getTailwindClasses(): string {
    const { colors, spacing } = this.preset;
    return `
      bg-white dark:bg-slate-900
      text-gray-900 dark:text-gray-100
      space-y-${spacing.unit}
    `;
  }

  private generateHTML(): string {
    const { title, description, includeHero, includeFeatures, includeFooter, layout } = this.designInput;
    const { colors } = this.preset;

    let content = '';

    if (includeHero) {
      content += `
    <section class="min-h-screen flex items-center justify-center" style="background: linear-gradient(135deg, ${colors.primary} 0%, ${colors.secondary} 100%);">
      <div class="text-center text-white p-8">
        <h1 class="text-5xl font-bold mb-4">${title}</h1>
        <p class="text-xl opacity-90">${description}</p>
      </div>
    </section>`;
    }

    if (includeFeatures) {
      const gridClass = this.getGridClass(layout);
      content += `
    <section class="py-16 px-4 sm:px-6 lg:px-8">
      <div class="max-w-6xl mx-auto">
        <h2 class="text-3xl font-bold text-center mb-12" style="color: ${colors.primary}">Features</h2>
        <div class="${gridClass} gap-8">
          <div class="p-6 rounded-lg" style="background: ${colors.accent}20; border-left: 4px solid ${colors.accent};">
            <h3 class="text-xl font-bold mb-2">Feature One</h3>
            <p class="opacity-75">Describe your first key feature here</p>
          </div>
          <div class="p-6 rounded-lg" style="background: ${colors.accent}20; border-left: 4px solid ${colors.accent};">
            <h3 class="text-xl font-bold mb-2">Feature Two</h3>
            <p class="opacity-75">Describe your second key feature here</p>
          </div>
          <div class="p-6 rounded-lg" style="background: ${colors.accent}20; border-left: 4px solid ${colors.accent};">
            <h3 class="text-xl font-bold mb-2">Feature Three</h3>
            <p class="opacity-75">Describe your third key feature here</p>
          </div>
        </div>
      </div>
    </section>`;
    }

    if (includeFooter) {
      content += `
    <footer style="background: ${colors.primary}; color: white;" class="py-8 px-4">
      <div class="max-w-6xl mx-auto text-center">
        <p>&copy; 2024 ${title}. All rights reserved.</p>
      </div>
    </footer>`;
    }

    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>${title}</title>
  <script src="https://cdn.tailwindcss.com"></script>
  <link rel="stylesheet" href="styles.css">
</head>
<body>${content}
</body>
</html>`;
  }

  private generateCSS(): string {
    const { colors, typography, borderRadius, spacing } = this.preset;

    return `:root {
  --primary: ${colors.primary};
  --secondary: ${colors.secondary};
  --accent: ${colors.accent};
  --background: ${colors.background};
  --text: ${colors.text};
  --spacing-unit: ${spacing.unit}px;
  --font-family: ${typography.fontFamily};
  --border-radius-sm: ${borderRadius.sm}px;
  --border-radius-md: ${borderRadius.md}px;
  --border-radius-lg: ${borderRadius.lg}px;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html {
  font-size: ${typography.baseFontSize}px;
  font-family: var(--font-family);
  color: var(--text);
  background-color: var(--background);
}

body {
  line-height: 1.6;
  color: var(--text);
}

h1, h2, h3, h4, h5, h6 {
  font-weight: 700;
  margin-bottom: calc(var(--spacing-unit) * 2);
  line-height: 1.2;
}

h1 { font-size: calc(1rem * ${typography.headingScale}${3}); }
h2 { font-size: calc(1rem * ${typography.headingScale}${2}); }
h3 { font-size: calc(1rem * ${typography.headingScale}); }

p {
  margin-bottom: var(--spacing-unit);
}

section {
  padding: calc(var(--spacing-unit) * 4);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--spacing-unit);
}`;
  }

  private generateJavaScript(): string {
    return `// Generated JavaScript
document.addEventListener('DOMContentLoaded', function() {
  console.log('Page loaded successfully');

  // Add smooth scroll behavior
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
      e.preventDefault();
      const target = document.querySelector(this.getAttribute('href'));
      if (target) {
        target.scrollIntoView({ behavior: 'smooth' });
      }
    });
  });

  // Add intersection observer for animations
  const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -100px 0px'
  };

  const observer = new IntersectionObserver(function(entries) {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('animate-in');
        observer.unobserve(entry.target);
      }
    });
  }, observerOptions);

  document.querySelectorAll('section').forEach(el => {
    observer.observe(el);
  });
});`;
  }

  private generateTypeScript(): string {
    return `// Generated TypeScript
interface PageConfig {
  title: string;
  description: string;
  theme: string;
}

const pageConfig: PageConfig = {
  title: '${this.designInput.title}',
  description: '${this.designInput.description}',
  theme: '${this.designInput.preset}',
};

function initializePage(config: PageConfig): void {
  console.log('Initializing page with config:', config);
  document.title = config.title;
}

document.addEventListener('DOMContentLoaded', () => {
  initializePage(pageConfig);
});

export { pageConfig, initializePage };`;
  }

  private generateReact(): string {
    const { colors } = this.preset;
    const { title, description } = this.designInput;

    return `import React, { useState } from 'react';

interface Feature {
  id: number;
  title: string;
  description: string;
}

export default function App() {
  const [activeFeature, setActiveFeature] = useState<number | null>(null);

  const features: Feature[] = [
    { id: 1, title: 'Feature One', description: 'Describe your first key feature' },
    { id: 2, title: 'Feature Two', description: 'Describe your second key feature' },
    { id: 3, title: 'Feature Three', description: 'Describe your third key feature' },
  ];

  return (
    <div className="min-h-screen flex flex-col">
      <section className="min-h-screen flex items-center justify-center text-white" style={{ background: \`linear-gradient(135deg, ${colors.primary} 0%, ${colors.secondary} 100%)\` }}>
        <div className="text-center p-8">
          <h1 className="text-5xl font-bold mb-4">${title}</h1>
          <p className="text-xl opacity-90">${description}</p>
        </div>
      </section>

      <section className="py-16 px-4 sm:px-6 lg:px-8">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl font-bold text-center mb-12" style={{ color: '${colors.primary}' }}>Features</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {features.map((feature) => (
              <div
                key={feature.id}
                className="p-6 rounded-lg cursor-pointer transition-all"
                style={{
                  background: \`${colors.accent}20\`,
                  borderLeft: \`4px solid ${colors.accent}\`,
                  opacity: activeFeature === null || activeFeature === feature.id ? 1 : 0.6,
                }}
                onClick={() => setActiveFeature(activeFeature === feature.id ? null : feature.id)}
              >
                <h3 className="text-xl font-bold mb-2">{feature.title}</h3>
                <p className="opacity-75">{feature.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <footer style={{ background: '${colors.primary}', color: 'white' }} className="py-8 px-4">
        <div className="max-w-6xl mx-auto text-center">
          <p>&copy; 2024 ${title}. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}`;
  }

  private generateVue(): string {
    const { colors } = this.preset;
    const { title, description } = this.designInput;

    return `<template>
  <div class="min-h-screen flex flex-col">
    <section class="min-h-screen flex items-center justify-center text-white" :style="{ background: \`linear-gradient(135deg, ${colors.primary} 0%, ${colors.secondary} 100%)\` }">
      <div class="text-center p-8">
        <h1 class="text-5xl font-bold mb-4">${title}</h1>
        <p class="text-xl opacity-90">${description}</p>
      </div>
    </section>

    <section class="py-16 px-4 sm:px-6 lg:px-8">
      <div class="max-w-6xl mx-auto">
        <h2 class="text-3xl font-bold text-center mb-12" :style="{ color: '${colors.primary}' }">Features</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div
            v-for="feature in features"
            :key="feature.id"
            class="p-6 rounded-lg cursor-pointer transition-all"
            :style="{
              background: \`${colors.accent}20\`,
              borderLeft: \`4px solid ${colors.accent}\`,
              opacity: activeFeature === null || activeFeature === feature.id ? 1 : 0.6,
            }"
            @click="toggleFeature(feature.id)"
          >
            <h3 class="text-xl font-bold mb-2">{{ feature.title }}</h3>
            <p class="opacity-75">{{ feature.description }}</p>
          </div>
        </div>
      </div>
    </section>

    <footer :style="{ background: '${colors.primary}', color: 'white' }" class="py-8 px-4">
      <div class="max-w-6xl mx-auto text-center">
        <p>&copy; 2024 ${title}. All rights reserved.</p>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

interface Feature {
  id: number;
  title: string;
  description: string;
}

const activeFeature = ref<number | null>(null);

const features: Feature[] = [
  { id: 1, title: 'Feature One', description: 'Describe your first key feature' },
  { id: 2, title: 'Feature Two', description: 'Describe your second key feature' },
  { id: 3, title: 'Feature Three', description: 'Describe your third key feature' },
];

function toggleFeature(id: number) {
  activeFeature.value = activeFeature.value === id ? null : id;
}
</script>`;
  }

  private getGridClass(layout: string): string {
    switch (layout) {
      case 'two-column':
        return 'grid grid-cols-1 md:grid-cols-2';
      case 'three-column':
        return 'grid grid-cols-1 md:grid-cols-3';
      default:
        return 'grid grid-cols-1';
    }
  }

  generateForLanguage(language: Language): string {
    switch (language) {
      case 'html':
        return this.generateHTML();
      case 'css':
        return this.generateCSS();
      case 'javascript':
        return this.generateJavaScript();
      case 'typescript':
        return this.generateTypeScript();
      case 'react':
        return this.generateReact();
      case 'vue':
        return this.generateVue();
      default:
        return '';
    }
  }

  generateForFramework(framework: Framework): CodeFile[] {
    const files: CodeFile[] = [];

    switch (framework) {
      case 'tailwind':
        files.push({
          name: 'index.html',
          language: 'html',
          content: this.generateHTML(),
        });
        files.push({
          name: 'styles.css',
          language: 'css',
          content: this.generateCSS(),
        });
        files.push({
          name: 'main.js',
          language: 'javascript',
          content: this.generateJavaScript(),
        });
        break;

      case 'react':
        files.push({
          name: 'App.tsx',
          language: 'typescript',
          content: this.generateReact(),
        });
        break;

      case 'vue':
        files.push({
          name: 'App.vue',
          language: 'vue',
          content: this.generateVue(),
        });
        break;
    }

    return files;
  }

  generateAll(): GeneratedCode {
    return {
      html: this.generateHTML(),
      css: this.generateCSS(),
      js: this.generateJavaScript(),
      ts: this.generateTypeScript(),
      jsx: this.generateReact(),
      tsx: this.generateReact(),
      vue: this.generateVue(),
    };
  }
}

// Export validation functions from validators module
export { validateCode, validateJavaScript, validateTypeScript, validateHTML, validateCSS, validateReact, validateVue } from './validators';
export type { ValidationResult } from './validators';
