export type Framework = 'tailwind' | 'react' | 'vue';

export interface FrameworkConfig {
  id: Framework;
  name: string;
  description: string;
  version: string;
  supportsLanguages: string[];
  defaultLanguage: string;
  features: string[];
}

export const FRAMEWORKS: Record<Framework, FrameworkConfig> = {
  tailwind: {
    id: 'tailwind',
    name: 'Tailwind CSS',
    description: 'Utility-first CSS framework',
    version: '3.3.0',
    supportsLanguages: ['html', 'css', 'javascript', 'typescript', 'react', 'vue'],
    defaultLanguage: 'html',
    features: ['responsive', 'dark-mode', 'animations', 'components'],
  },
  react: {
    id: 'react',
    name: 'React',
    description: 'A JavaScript library for building UI with components',
    version: '18.2.0',
    supportsLanguages: ['javascript', 'typescript'],
    defaultLanguage: 'typescript',
    features: ['hooks', 'components', 'state-management', 'jsx'],
  },
  vue: {
    id: 'vue',
    name: 'Vue',
    description: 'The Progressive JavaScript Framework',
    version: '3.3.0',
    supportsLanguages: ['javascript', 'typescript'],
    defaultLanguage: 'typescript',
    features: ['composition-api', 'single-file-components', 'reactivity', 'directives'],
  },
};

export const FRAMEWORK_LIST = Object.values(FRAMEWORKS);

export interface FrameworkTemplate {
  name: string;
  imports: string[];
  setup: string;
}

export const FRAMEWORK_TEMPLATES: Record<Framework, FrameworkTemplate> = {
  tailwind: {
    name: 'Tailwind CSS',
    imports: [],
    setup: `@tailwind base;
@tailwind components;
@tailwind utilities;`,
  },
  react: {
    name: 'React',
    imports: ["import React from 'react';"],
    setup: `export default function App() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-8">
      <h1 className="text-4xl font-bold text-gray-900">Hello from React</h1>
    </div>
  );
}`,
  },
  vue: {
    name: 'Vue',
    imports: [],
    setup: `<template>
  <div class="min-h-screen bg-gradient-to-br from-green-50 to-teal-100 p-8">
    <h1 class="text-4xl font-bold text-gray-900">Hello from Vue</h1>
  </div>
</template>

<script setup lang="ts">
</script>`,
  },
};
