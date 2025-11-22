# Figma to Code Generator

A modern web application that generates HTML, CSS, JavaScript, TypeScript, React, and Vue code from design inputs and presets.

## Features

- **Multi-Language Support**: Generate code in HTML, CSS, JavaScript, TypeScript, React, and Vue
- **Framework Integration**: Built-in support for Tailwind CSS, React, and Vue frameworks
- **Design Presets**: Multiple design presets (Minimal, Modern, Organic, Dark) with customizable colors, typography, and spacing
- **Real-time Generation**: Code is generated instantly as you change any setting
- **Syntax Validation**: All generated code is validated using language-specific validators
  - JavaScript validation via `new Function`
  - TypeScript validation with type stripping
  - HTML validation via DOM Parser
  - CSS validation via style injection
  - React/JSX validation
  - Vue SFC validation
- **Multi-File Output**: Download generated code as individual files
- **Copy to Clipboard**: Easily copy any generated code snippet
- **Responsive UI**: Beautiful, responsive interface built with React and Tailwind CSS

## Tech Stack

- **Frontend**: Next.js 14, React 18, TypeScript
- **State Management**: Zustand
- **Styling**: Tailwind CSS
- **UI Components**: Radix UI
- **Icons**: Lucide React

## Project Structure

```
src/
├── app/                          # Next.js app directory
│   ├── layout.tsx               # Root layout
│   ├── page.tsx                 # Home page
│   └── globals.css              # Global styles
├── components/                   # React components
│   ├── tech-stack-selector.tsx  # Language/framework selection
│   ├── design-input-panel.tsx   # Design configuration
│   ├── code-display.tsx         # Generated code display
│   └── ui/                      # UI component library
├── data/                         # Domain data
│   ├── languages.ts             # Language definitions
│   ├── frameworks.ts            # Framework definitions
│   ├── presets.ts               # Design presets
│   └── index.ts                 # Data exports
├── lib/                          # Utilities and services
│   ├── generator.ts             # Code generator service
│   ├── validators.ts            # Code validation functions
│   ├── utils.ts                 # Common utilities
│   └── index.ts                 # Library exports
└── stores/                       # State management
    ├── generatorStore.ts        # Zustand store
    └── index.ts                 # Store exports
```

## Getting Started

### Prerequisites

- Node.js 18+ 
- npm or yarn

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd web/figma-to-code
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm run dev
```

4. Open [http://localhost:3000](http://localhost:3000) in your browser

## Usage

### Basic Workflow

1. **Select Languages**: Choose one or more languages (HTML, CSS, JavaScript, TypeScript, React, Vue)
2. **Choose Framework** (Optional): Select a framework (Tailwind CSS, React, Vue)
3. **Configure Design**: 
   - Select a design preset
   - Enter site title and description
   - Choose layout (single, two, or three column)
   - Toggle sections (hero, features, footer)
4. **View Generated Code**: Code is generated automatically and displayed with syntax validation
5. **Copy or Download**: Copy individual snippets or download as files

### Design Presets

- **Minimal**: Clean, simple black and white design
- **Modern**: Contemporary gradient-based design with vibrant colors
- **Organic**: Warm and friendly design with earth tones
- **Dark**: Dark theme with neon accents

## API Reference

### useGeneratorStore()

Zustand store for managing generator state.

```typescript
const store = useGeneratorStore();

// State
store.selectedLanguages;      // Language[]
store.selectedFramework;       // Framework | null
store.designInput;            // DesignInput
store.generatedCode;          // Record<string, string>
store.generatedFiles;         // CodeFile[]
store.validationErrors;       // Record<string, string | null>

// Actions
store.toggleLanguage(language);
store.setLanguages(languages);
store.setFramework(framework);
store.updateDesignInput(input);
store.generateCode();
store.clearAll();
store.resetToDefaults();
```

### CodeGenerator

Main service for code generation.

```typescript
import { CodeGenerator } from '@/lib/generator';

const generator = new CodeGenerator(designInput);

// Generate for specific language
const htmlCode = generator.generateForLanguage('html');

// Generate for framework
const files = generator.generateForFramework('react');

// Generate all at once
const allCode = generator.generateAll();
```

### Validators

Language-specific code validators.

```typescript
import {
  validateCode,
  validateJavaScript,
  validateTypeScript,
  validateHTML,
  validateCSS,
  validateReact,
  validateVue
} from '@/lib/validators';

const result = validateCode('javascript', 'console.log("hello")');
// { isValid: true }

const result = validateTypeScript('const x: number = "string";');
// { isValid: false, error: '...' }
```

## Configuration

### Available Languages

- `html` - HTML5
- `css` - CSS3
- `javascript` - ES6+ JavaScript
- `typescript` - TypeScript
- `react` - React with JSX/TSX
- `vue` - Vue 3 Single File Components

### Available Frameworks

- `tailwind` - Tailwind CSS v3.3.0
- `react` - React v18.2.0
- `vue` - Vue v3.3.0

## Validation

The application validates all generated code using language-specific validators:

### JavaScript Validation
Uses `new Function()` to verify syntax validity.

### TypeScript Validation
Strips type annotations and validates as JavaScript, with additional checks for TypeScript-specific syntax.

### HTML Validation
Uses the DOM Parser API to validate HTML structure.

### CSS Validation
Injects CSS into a `<style>` element and verifies browser acceptance.

### React/JSX Validation
Validates as JavaScript with JSX extensions.

### Vue Validation
Checks for proper Single File Component structure.

## Development

### Build
```bash
npm run build
```

### Type Check
```bash
npm run type-check
```

### Lint
```bash
npm run lint
```

### Format
Code is formatted using Prettier (configured via ESLint)

## Project Acceptance Criteria

✅ **Language/Framework Data**: Comprehensive definitions in `src/data/`
✅ **Generator Service**: Full implementation in `src/lib/generator.ts`
✅ **Multi-file Output**: Support for HTML, CSS, JS, TS, JSX, TSX, Vue
✅ **Zustand Store**: Complete state management in `src/stores/generatorStore.ts`
✅ **Real-time Updates**: Code generates immediately on any setting change
✅ **Syntax Validation**:
  - JavaScript: `new Function` validation
  - TypeScript: Type stripping + JS validation
  - HTML: DOM Parser validation
  - CSS: Style injection validation
  - React/Vue: Proper structure validation

## Browser Support

- Chrome/Edge 90+
- Firefox 88+
- Safari 14+

## License

MIT

## Contributing

Contributions are welcome! Please submit a pull request with your changes.
