# Quick Start Guide

## 5-Minute Setup

### 1. Install Dependencies
```bash
cd web/figma-to-code
npm install
```

### 2. Start Development Server
```bash
npm run dev
```
Open http://localhost:3000

### 3. Start Generating Code!

**Select Languages:**
- Click HTML, CSS, JavaScript, TypeScript, React, or Vue buttons
- Each selection automatically triggers code generation

**Select Framework (Optional):**
- Click Tailwind, React, or Vue
- Framework code is generated separately

**Configure Design:**
- Change preset (Minimal, Modern, Organic, Dark)
- Update title and description
- Toggle sections (hero, features, footer)
- Change layout (single, two, three column)

**View Generated Code:**
- Code appears automatically in the right panel
- Each language/file has its own tab
- Green checkmark = valid code, red X = validation error

**Copy Code:**
- Click "Copy Code" button below any code snippet
- Paste directly into your project

## Core Concepts

### Zustand Store Pattern
```typescript
import useGeneratorStore from '@/stores/generatorStore';

function MyComponent() {
  const { selectedLanguages, toggleLanguage } = useGeneratorStore();
  
  return (
    <button onClick={() => toggleLanguage('javascript')}>
      Toggle JavaScript
    </button>
  );
}
```

### Adding a New Language

1. Add to `src/data/languages.ts`:
```typescript
export const LANGUAGES: Record<Language, LanguageConfig> = {
  // ... existing languages
  newlang: {
    id: 'newlang',
    name: 'New Language',
    description: 'Description',
    extension: '.nl',
    mimeType: 'text/newlang',
    supportsFrameworks: [],
  },
};
```

2. Add generation method to `CodeGenerator` in `src/lib/generator.ts`:
```typescript
private generateNewLang(): string {
  // Generate code for new language
  return `// Generated code`;
}
```

3. Add to `generateForLanguage()` switch statement:
```typescript
case 'newlang':
  return this.generateNewLang();
```

4. Add validator to `src/lib/validators.ts`:
```typescript
export function validateNewLang(code: string): ValidationResult {
  // Validation logic
  return { isValid: true };
}
```

### Adding a New Framework

1. Add to `src/data/frameworks.ts`:
```typescript
export const FRAMEWORKS: Record<Framework, FrameworkConfig> = {
  // ... existing frameworks
  newfw: {
    id: 'newfw',
    name: 'New Framework',
    description: 'Description',
    version: '1.0.0',
    supportsLanguages: ['javascript', 'typescript'],
    defaultLanguage: 'javascript',
    features: ['feature1', 'feature2'],
  },
};
```

2. Add generation method to `CodeGenerator`:
```typescript
private generateNewFramework(): CodeFile[] {
  return [
    {
      name: 'App.js',
      language: 'javascript',
      content: '// Generated code',
    },
  ];
}
```

3. Add to `generateForFramework()` switch statement:
```typescript
case 'newfw':
  return this.generateNewFramework();
```

### Adding a New Design Preset

1. Add to `src/data/presets.ts`:
```typescript
export const DESIGN_PRESETS: Record<string, DesignPreset> = {
  // ... existing presets
  newpreset: {
    id: 'newpreset',
    name: 'New Preset',
    description: 'Description',
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
    spacing: { unit: 4 },
    borderRadius: { sm: 2, md: 4, lg: 8 },
  },
};
```

## Common Tasks

### View Generated Code
1. Select a language
2. Code appears in right panel automatically
3. Click tab to switch between languages

### Validate Generated Code
Code is validated automatically:
- JavaScript: Uses `new Function` syntax check
- TypeScript: Type stripping + JavaScript validation
- HTML: DOM Parser validation
- CSS: Style injection validation
- Red error text = validation failed

### Download Generated Files
Click "Copy Code" button and paste into your files. In future, we'll add direct download support.

### Reset Everything
1. Click Settings (not implemented yet, use reset manually)
2. Or refresh the page

## Troubleshooting

**Code not generating?**
- Ensure at least one language is selected
- Check browser console for errors
- Try refreshing the page

**Validation errors?**
- Red error text shows validation issues
- JavaScript: Check syntax is valid
- TypeScript: Ensure valid type annotations
- HTML: Check for unclosed tags
- CSS: Check for valid rules

**Performance issues?**
- Generating for many languages may be slow
- Try selecting fewer languages
- Clear browser cache if issues persist

## File Structure Reference

```
src/
├── data/              # Language/framework/preset definitions
├── lib/               # Code generation and validation
├── stores/            # Zustand state management
├── components/        # React UI components
└── app/              # Next.js app structure
```

## Testing

Test the generator locally:
```bash
# Test JavaScript validation
const result = validateJavaScript('console.log("test")');

# Test TypeScript validation
const result = validateTypeScript('const x: number = 1;');

# Test HTML validation
const result = validateHTML('<html><body></body></html>');

# Test CSS validation
const result = validateCSS('body { color: red; }');
```

## Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [Zustand Documentation](https://github.com/pmndrs/zustand)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [TypeScript Documentation](https://www.typescriptlang.org/docs)

## Need Help?

Check the main [README.md](./README.md) for full documentation.
