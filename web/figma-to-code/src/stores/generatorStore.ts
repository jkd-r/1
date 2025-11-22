'use client';

import { create } from 'zustand';
import { Language } from '@/data/languages';
import { Framework } from '@/data/frameworks';
import { DesignInput, DEFAULT_DESIGN_INPUT } from '@/data/presets';
import { CodeGenerator, CodeFile, validateCode } from '@/lib/generator';

export interface GeneratorStore {
  // State
  selectedLanguages: Language[];
  selectedFramework: Framework | null;
  designInput: DesignInput;
  generatedCode: Record<string, string>;
  generatedFiles: CodeFile[];
  validationErrors: Record<string, string | null>;
  isGenerating: boolean;

  // Actions - Language & Framework
  toggleLanguage: (language: Language) => void;
  setLanguages: (languages: Language[]) => void;
  setFramework: (framework: Framework | null) => void;

  // Actions - Design Input
  updateDesignInput: (input: Partial<DesignInput>) => void;
  setDesignInput: (input: DesignInput) => void;

  // Actions - Generation
  generateCode: () => void;
  generateForLanguage: (language: Language) => void;
  generateForFramework: (framework: Framework) => void;

  // Actions - Settings
  clearAll: () => void;
  resetToDefaults: () => void;
}

const useGeneratorStore = create<GeneratorStore>((set, get) => ({
  // Initial state
  selectedLanguages: [],
  selectedFramework: null,
  designInput: DEFAULT_DESIGN_INPUT,
  generatedCode: {},
  generatedFiles: [],
  validationErrors: {},
  isGenerating: false,

  // Language & Framework actions
  toggleLanguage: (language: Language) => {
    set((state) => {
      const newLanguages = state.selectedLanguages.includes(language)
        ? state.selectedLanguages.filter((l) => l !== language)
        : [...state.selectedLanguages, language];
      return { selectedLanguages: newLanguages };
    });
    // Auto-generate when language is toggled
    get().generateCode();
  },

  setLanguages: (languages: Language[]) => {
    set({ selectedLanguages: languages });
    // Auto-generate when languages are set
    get().generateCode();
  },

  setFramework: (framework: Framework | null) => {
    set({ selectedFramework: framework });
    // Auto-generate when framework is changed
    get().generateCode();
  },

  // Design input actions
  updateDesignInput: (input: Partial<DesignInput>) => {
    set((state) => ({
      designInput: { ...state.designInput, ...input },
    }));
    // Auto-generate when design input changes
    get().generateCode();
  },

  setDesignInput: (input: DesignInput) => {
    set({ designInput: input });
    // Auto-generate when design input is set
    get().generateCode();
  },

  // Generation actions
  generateCode: () => {
    set({ isGenerating: true });

    try {
      const state = get();
      const generator = new CodeGenerator(state.designInput);

      const newGeneratedCode: Record<string, string> = {};
      const newValidationErrors: Record<string, string | null> = {};

      // Generate for each selected language
      for (const language of state.selectedLanguages) {
        newGeneratedCode[language] = generator.generateForLanguage(language);
        // Validate generated code
        const { isValid, error } = validateGeneratedCode(language, newGeneratedCode[language]);
        newValidationErrors[language] = isValid ? null : error || 'Validation failed';
      }

      // Generate for framework if selected
      let generatedFiles: CodeFile[] = [];
      if (state.selectedFramework) {
        generatedFiles = generator.generateForFramework(state.selectedFramework);
        // Validate framework files
        for (const file of generatedFiles) {
          const { isValid, error } = validateGeneratedCode(file.language as Language, file.content);
          newValidationErrors[file.name] = isValid ? null : error || 'Validation failed';
        }
      }

      set({
        generatedCode: newGeneratedCode,
        generatedFiles,
        validationErrors: newValidationErrors,
        isGenerating: false,
      });
    } catch (error) {
      set({
        validationErrors: {
          error: error instanceof Error ? error.message : 'Generation failed',
        },
        isGenerating: false,
      });
    }
  },

  generateForLanguage: (language: Language) => {
    set((state) => {
      const generator = new CodeGenerator(state.designInput);
      const code = generator.generateForLanguage(language);
      const { isValid, error } = validateGeneratedCode(language, code);

      return {
        generatedCode: {
          ...state.generatedCode,
          [language]: code,
        },
        validationErrors: {
          ...state.validationErrors,
          [language]: isValid ? null : error || 'Validation failed',
        },
      };
    });
  },

  generateForFramework: (framework: Framework) => {
    set((state) => {
      const generator = new CodeGenerator(state.designInput);
      const files = generator.generateForFramework(framework);
      const newValidationErrors: Record<string, string | null> = { ...state.validationErrors };

      for (const file of files) {
        const { isValid, error } = validateGeneratedCode(file.language as Language, file.content);
        newValidationErrors[file.name] = isValid ? null : error || 'Validation failed';
      }

      return {
        generatedFiles: files,
        validationErrors: newValidationErrors,
      };
    });
  },

  // Settings actions
  clearAll: () => {
    set({
      selectedLanguages: [],
      selectedFramework: null,
      generatedCode: {},
      generatedFiles: [],
      validationErrors: {},
    });
  },

  resetToDefaults: () => {
    set({
      selectedLanguages: [],
      selectedFramework: null,
      designInput: DEFAULT_DESIGN_INPUT,
      generatedCode: {},
      generatedFiles: [],
      validationErrors: {},
    });
  },
}));

function validateGeneratedCode(
  language: string,
  code: string,
): { isValid: boolean; error?: string } {
  const result = validateCode(language, code);
  return {
    isValid: result.isValid,
    error: result.error,
  };
}

export default useGeneratorStore;
