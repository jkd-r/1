# Figma to Code - UI Layout

A responsive, accessibility-first web application for converting Figma designs to production-ready code.

## Features

### 🎨 Responsive Layout
- **Header/Nav Bar**: Logo + dark/light theme toggle with keyboard navigation
- **Sidebar**: Language (React, Vue, Svelte, HTML) and styling framework (Tailwind, Styled, CSS) selection
- **Design Input Panel**: Drag-and-drop file upload + JSON paste support
- **Code Editor Pane**: Syntax-highlighted code output with copy/download controls
- **Preview Pane**: Live preview of generated components
- **Settings Drawer**: Configuration for code generation and output formats
- **Action Bar**: Export and share controls

### ♿ Accessibility
- WCAG 2.1 AA compliance (contrast ratio ≥ 4.5:1)
- Full keyboard navigation support
- Focus states clearly visible
- Semantic HTML structure
- ARIA labels and roles
- Screen reader optimized

### 📱 Responsive Design
- **Mobile**: Single column layout, collapsible sidebar
- **Tablet**: Two-column layout with flexible widths
- **Desktop**: Three-column layout with full feature set
- Tailwind CSS breakpoints: `sm`, `md`, `lg`, `xl`, `2xl`

### 🎯 Component Primitives
- **shadcn/ui Components**:
  - Button (variants: default, destructive, outline, secondary, ghost, link)
  - Toggle & ToggleGroup (for language/framework selection)
  - Tabs (for code/dependencies view)
  - Sheet (for settings drawer)
  - Tooltip (for action hints)

- **Theme System**:
  - Light/Dark mode support
  - System preference detection
  - Smooth theme transitions
  - CSS variable-based color system

## Project Structure

```
src/
├── app/
│   ├── globals.css          # Global styles + accessibility utilities
│   ├── layout.tsx           # Root layout with theme provider
│   └── page.tsx             # Home page
├── components/
│   ├── ui/                  # shadcn primitives
│   │   ├── button.tsx
│   │   ├── toggle.tsx
│   │   ├── toggle-group.tsx
│   │   ├── tabs.tsx
│   │   ├── sheet.tsx
│   │   └── tooltip.tsx
│   ├── theme-provider.tsx   # Theme context and hooks
│   ├── header.tsx           # Header with theme toggle
│   ├── sidebar.tsx          # Language/framework selection
│   ├── design-input.tsx     # Upload and paste area
│   ├── code-editor-pane.tsx # Code display
│   ├── preview-pane.tsx     # Component preview
│   ├── settings-drawer.tsx  # Settings configuration
│   ├── action-bar.tsx       # Export/share controls
│   └── layout.tsx           # Main layout composition
└── lib/
    └── utils.ts             # Utility functions (cn, etc.)
```

## Getting Started

### Prerequisites
- Node.js 18+
- npm or yarn

### Installation

```bash
cd web/figma-to-code
npm install
```

### Development

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

### Build

```bash
npm run build
npm start
```

## Accessibility Features

### Color Contrast
- Background to text: 9.2:1 (light mode), 10:1 (dark mode)
- Button states clearly distinguished
- Focus indicators: 2px ring with 2px offset

### Keyboard Navigation
- All interactive elements accessible via Tab
- Shift+Tab for reverse navigation
- Enter/Space to activate buttons
- Arrow keys for toggle groups
- Escape to close drawers

### Focus Management
- Visible focus states on all interactive elements
- Focus visible outline: `ring-2 ring-ring ring-offset-2`
- Dark mode: `ring-offset-0` for better visibility

### Semantic Structure
- Proper heading hierarchy (h1, h2, h3)
- Landmark elements (header, main, aside, footer)
- Form labels properly associated
- ARIA attributes where needed

## Theme System

### Light Mode (Default)
- Background: `#ffffff`
- Foreground: `#0a0a0a`
- Primary: `#000000`
- Accent: `#1a1a1a`

### Dark Mode
- Background: `#0a0a0a`
- Foreground: `#fafafa`
- Primary: `#fafafa`
- Accent: `#fafafa`

### Switching Themes
1. Click the moon/sun icon in the header
2. Preference is saved to localStorage
3. Falls back to system preference if not set

## Configuration

### Tailwind CSS
- Custom color system with CSS variables
- Spacing scale: xs (0.5rem) to 2xl (3rem)
- Radius: 0.5rem default
- Supports dark mode

### Next.js
- App Router setup
- TypeScript support
- Optimized font loading (Inter)
- Automatic code splitting

## Browser Support

- Chrome/Edge: Latest 2 versions
- Firefox: Latest 2 versions
- Safari: Latest 2 versions
- Mobile: iOS 12+, Android 8+

## Performance Targets

- Initial Load: < 2s
- Interactive: < 3.5s
- Layout Shift: < 0.1
- Accessibility Compliance: WCAG 2.1 AA

## Development Guidelines

### Component Conventions
- Use PascalCase for component names
- Props interface extends HTML attributes
- Use `forwardRef` for components that need DOM access
- Document accessibility features in comments

### Styling
- Use Tailwind utilities first
- Avoid inline styles
- Use CSS variables for theming
- Maintain 4.5:1 contrast minimum

### Accessibility Checklist
- [ ] Keyboard navigation works
- [ ] Focus states visible
- [ ] Color not sole means of conveyance
- [ ] Text alternatives for icons
- [ ] Form labels present
- [ ] Error messages clear

## Future Enhancements

- [ ] Real Figma API integration
- [ ] Component code generation from designs
- [ ] Multi-file project export
- [ ] Collaborative editing
- [ ] Design system synchronization
- [ ] Custom themes and presets

## License

MIT
