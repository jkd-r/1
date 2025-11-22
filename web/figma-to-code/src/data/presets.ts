export interface DesignPreset {
  id: string;
  name: string;
  description: string;
  colors: {
    primary: string;
    secondary: string;
    accent: string;
    background: string;
    text: string;
  };
  typography: {
    fontFamily: string;
    baseFontSize: number;
    headingScale: number;
  };
  spacing: {
    unit: number;
  };
  borderRadius: {
    sm: number;
    md: number;
    lg: number;
  };
}

export const DESIGN_PRESETS: Record<string, DesignPreset> = {
  minimal: {
    id: 'minimal',
    name: 'Minimal',
    description: 'Clean and simple design',
    colors: {
      primary: '#000000',
      secondary: '#666666',
      accent: '#0066cc',
      background: '#ffffff',
      text: '#1a1a1a',
    },
    typography: {
      fontFamily: "'Inter', sans-serif",
      baseFontSize: 16,
      headingScale: 1.25,
    },
    spacing: {
      unit: 4,
    },
    borderRadius: {
      sm: 2,
      md: 4,
      lg: 8,
    },
  },
  modern: {
    id: 'modern',
    name: 'Modern',
    description: 'Contemporary gradient-based design',
    colors: {
      primary: '#3b82f6',
      secondary: '#8b5cf6',
      accent: '#ec4899',
      background: '#f8fafc',
      text: '#0f172a',
    },
    typography: {
      fontFamily: "'Poppins', sans-serif",
      baseFontSize: 16,
      headingScale: 1.3,
    },
    spacing: {
      unit: 6,
    },
    borderRadius: {
      sm: 4,
      md: 8,
      lg: 12,
    },
  },
  organic: {
    id: 'organic',
    name: 'Organic',
    description: 'Warm and friendly design',
    colors: {
      primary: '#f59e0b',
      secondary: '#10b981',
      accent: '#f97316',
      background: '#fef3c7',
      text: '#78350f',
    },
    typography: {
      fontFamily: "'Sora', sans-serif",
      baseFontSize: 17,
      headingScale: 1.35,
    },
    spacing: {
      unit: 5,
    },
    borderRadius: {
      sm: 6,
      md: 12,
      lg: 16,
    },
  },
  dark: {
    id: 'dark',
    name: 'Dark',
    description: 'Dark theme with vibrant accents',
    colors: {
      primary: '#06b6d4',
      secondary: '#a855f7',
      accent: '#f43f5e',
      background: '#0f172a',
      text: '#f1f5f9',
    },
    typography: {
      fontFamily: "'IBM Plex Sans', sans-serif",
      baseFontSize: 16,
      headingScale: 1.25,
    },
    spacing: {
      unit: 4,
    },
    borderRadius: {
      sm: 3,
      md: 6,
      lg: 10,
    },
  },
};

export const PRESET_LIST = Object.values(DESIGN_PRESETS);

export interface DesignInput {
  preset: string;
  title: string;
  description: string;
  includeHero: boolean;
  includeFeatures: boolean;
  includeFooter: boolean;
  layout: 'single-column' | 'two-column' | 'three-column';
}

export const DEFAULT_DESIGN_INPUT: DesignInput = {
  preset: 'modern',
  title: 'My Website',
  description: 'Welcome to my awesome website',
  includeHero: true,
  includeFeatures: true,
  includeFooter: true,
  layout: 'single-column',
};
