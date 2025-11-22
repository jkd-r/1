# Quick Start Guide - Figma to Code UI Layout

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

Open [http://localhost:3000](http://localhost:3000) in your browser.

### 3. Test Key Features

#### Theme Toggle
- Click the moon/sun icon in the header
- Try switching between light/dark modes
- Your preference is saved to localStorage

#### Responsive Layout
- Resize browser to test breakpoints:
  - Mobile: < 640px (sidebar collapses)
  - Tablet: 640px - 1024px (two columns)
  - Desktop: > 1024px (three columns)
- Click menu button (📱 mobile only) to toggle sidebar

#### Language/Framework Selection
- Open sidebar (or scroll on mobile)
- Click language options (React, Vue, Svelte, HTML)
- Click styling options (Tailwind, Styled, CSS)
- Try preset buttons for quick selection

#### Design Upload
- Drag and drop a JSON file onto the "Drop your design here" area
- Or click to browse
- Or paste JSON directly in the textarea

#### Code Interaction
- Click copy icon to copy code to clipboard
- Click download icon to export code file
- Switch tabs to see "Code" and "Dependencies"

#### Preview
- Live preview renders in iframe
- Click refresh icon to reload
- Click fullscreen icon to expand preview

#### Settings
- Click gear icon in action bar
- Explore configuration options
- Note: These are UI placeholders; functionality can be integrated later

#### Accessibility
- Test keyboard navigation (Tab through all elements)
- Press Escape to close drawers
- Look for visible focus states on all interactive elements
- Try a screen reader to verify announcements

## File Structure Overview

```
web/figma-to-code/
├── src/
│   ├── app/                    # Next.js app directory
│   │   ├── layout.tsx          # Root layout
│   │   ├── page.tsx            # Home page
│   │   └── globals.css         # Global styles
│   ├── components/             # React components
│   │   ├── ui/                 # shadcn components
│   │   ├── header.tsx
│   │   ├── sidebar.tsx
│   │   ├── design-input.tsx
│   │   ├── code-editor-pane.tsx
│   │   ├── preview-pane.tsx
│   │   ├── settings-drawer.tsx
│   │   ├── action-bar.tsx
│   │   ├── layout.tsx
│   │   └── theme-provider.tsx
│   └── lib/
│       └── utils.ts            # Utility functions
├── package.json
├── tsconfig.json
├── next.config.js
├── tailwind.config.ts
├── postcss.config.js
├── .eslintrc.json
├── .gitignore
├── README.md
├── ACCESSIBILITY.md
└── QUICK_START.md (this file)
```

## Key Technologies

| Technology | Version | Purpose |
|---|---|---|
| Next.js | ^14.0.0 | React framework |
| React | ^18.2.0 | UI library |
| TypeScript | ^5.2.0 | Type safety |
| Tailwind CSS | ^3.3.0 | Utility styling |
| Radix UI | Latest | Accessible primitives |
| Lucide React | ^0.263.0 | Icons |

## Component Hierarchy

```
RootLayout
└── ThemeProvider
    └── TooltipProvider
        └── Layout (main.tsx)
            ├── Header
            │   ├── Menu Toggle
            │   ├── Logo
            │   └── Theme Toggle
            ├── Sidebar
            │   ├── Language Selection
            │   ├── Framework Selection
            │   └── Presets
            ├── Main Content
            │   ├── DesignInput Panel
            │   │   ├── Drag/Drop Area
            │   │   ├── Paste Textarea
            │   │   └── Action Buttons
            │   ├── CodeEditorPane
            │   │   ├── Tabs (Code/Dependencies)
            │   │   ├── Copy Button
            │   │   └── Download Button
            │   └── PreviewPane
            │       ├── Preview Controls
            │       └── iframe (Live Preview)
            └── ActionBar
                ├── Settings Drawer
                ├── Share Button
                └── Export Button
```

## Development Workflow

### Adding a New Component

1. Create component file in `src/components/`:
   ```tsx
   'use client'
   
   import React from 'react'
   import { Button } from '@/components/ui/button'
   
   export function MyComponent() {
     return <div>Component</div>
   }
   ```

2. Export from layout or import directly

3. Add Tailwind classes for styling

4. Include accessibility attributes (aria-labels, roles, etc.)

### Styling with Tailwind

- Use utility classes: `className="p-4 bg-slate-100 text-lg"`
- Use responsive prefixes: `className="w-full lg:w-1/3"`
- Use hover/focus states: `className="hover:bg-slate-200 focus-visible:ring-2"`
- Color system: `bg-background text-foreground` (theme-aware)

### Theme Customization

Colors use CSS variables in `globals.css`:

```css
:root {
  --background: 0 0% 100%;
  --foreground: 0 0% 3.6%;
  /* ... */
}

.dark {
  --background: 0 0% 3.6%;
  --foreground: 0 0% 98%;
  /* ... */
}
```

Use in Tailwind: `bg-background text-foreground`

### Building & Deployment

```bash
# Type check
npm run type-check

# Lint
npm run lint

# Build production
npm run build

# Start production server
npm start
```

## Accessibility Testing Quick Tips

### Manual Keyboard Test
1. Unplug mouse
2. Use Tab to navigate
3. Verify focus always visible
4. Press Escape to close modals
5. Use arrow keys for toggles

### Browser DevTools
- Chrome: Lighthouse → Accessibility
- Firefox: Accessibility Inspector
- Edge: Accessibility Inspector

### Screen Reader Test
- **Mac**: VoiceOver (Cmd+F5)
- **Windows**: NVDA (free download)
- **Windows**: JAWS (commercial)

### Contrast Checking
- Install: WAVE Web Accessibility Evaluation Tool
- Or use: Contrast Ratio tool

## Common Tasks

### Update Color Palette
1. Edit `src/app/globals.css` (CSS variables)
2. Verify contrast ratios in `ACCESSIBILITY.md`
3. Update Tailwind config if needed

### Add New Feature
1. Create component in `src/components/`
2. Import in `src/components/layout.tsx`
3. Add Tailwind styling
4. Include accessibility features (aria-labels, keyboard nav)
5. Test with keyboard and screen reader

### Customize Theme
Edit `tailwind.config.ts`:
```ts
theme: {
  extend: {
    spacing: { /* ... */ },
    colors: { /* ... */ },
  }
}
```

## Troubleshooting

### Port 3000 Already in Use
```bash
npm run dev -- -p 3001
```

### Tailwind Classes Not Applying
- Ensure file is in `content` array in `tailwind.config.ts`
- Restart dev server
- Check for typos in class names

### Theme Not Changing
- Check localStorage in DevTools
- Verify `ThemeProvider` wraps app
- Check `dark` class on `<html>` element

### Focus Indicator Not Visible
- Ensure element has `focus-visible` class or similar
- Check z-index doesn't hide focus ring
- Verify contrast ratio ≥ 4.5:1

## Next Steps

1. **Integrate Figma API**: Connect to real Figma files
2. **Add Code Generation**: Implement design-to-code conversion
3. **Multi-File Export**: Support exporting multiple files
4. **Collaboration**: Add real-time collaboration features
5. **Design System**: Build custom component libraries

## Support Resources

- **Docs**: See README.md and ACCESSIBILITY.md
- **Tailwind**: https://tailwindcss.com/docs
- **Next.js**: https://nextjs.org/docs
- **Radix UI**: https://www.radix-ui.com/primitives/docs
- **Accessibility**: https://www.w3.org/WAI/WCAG21/quickref/

---

**Need help?** Check the component files for inline comments and examples.
