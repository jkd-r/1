export type Language = 'html' | 'css' | 'javascript' | 'typescript' | 'react' | 'vue';

export interface LanguageConfig {
  id: Language;
  name: string;
  description: string;
  extension: string;
  mimeType: string;
  supportsFrameworks: string[];
}

export const LANGUAGES: Record<Language, LanguageConfig> = {
  html: {
    id: 'html',
    name: 'HTML',
    description: 'HyperText Markup Language',
    extension: '.html',
    mimeType: 'text/html',
    supportsFrameworks: [],
  },
  css: {
    id: 'css',
    name: 'CSS',
    description: 'Cascading Style Sheets',
    extension: '.css',
    mimeType: 'text/css',
    supportsFrameworks: ['tailwind'],
  },
  javascript: {
    id: 'javascript',
    name: 'JavaScript',
    description: 'JavaScript (ES6+)',
    extension: '.js',
    mimeType: 'text/javascript',
    supportsFrameworks: ['react', 'vue'],
  },
  typescript: {
    id: 'typescript',
    name: 'TypeScript',
    description: 'TypeScript',
    extension: '.ts',
    mimeType: 'application/typescript',
    supportsFrameworks: ['react', 'vue'],
  },
  react: {
    id: 'react',
    name: 'React',
    description: 'React with JSX/TSX',
    extension: '.tsx',
    mimeType: 'text/typescript',
    supportsFrameworks: [],
  },
  vue: {
    id: 'vue',
    name: 'Vue',
    description: 'Vue Single File Component',
    extension: '.vue',
    mimeType: 'text/plain',
    supportsFrameworks: [],
  },
};

export const LANGUAGE_LIST = Object.values(LANGUAGES);
