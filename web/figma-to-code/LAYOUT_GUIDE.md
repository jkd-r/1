# UI Layout Guide - Detailed Component Reference

## Overview

This document provides a comprehensive guide to the layout structure, responsive behavior, and component organization of the Figma to Code application.

## Layout Architecture

### Main Grid Structure

The application uses a flexible grid-based layout that adapts to different screen sizes:

```
┌─────────────────────────────────────────────────────────────────┐
│                          HEADER                                  │
│  [Menu] [Logo] [Title]                    [Theme Toggle]        │
├────────────┬────────────────────────────────────────────────────┤
│            │                                                      │
│  SIDEBAR   │                    MAIN CONTENT                    │
│            │  ┌──────────────┬──────────────┬──────────────┐    │
│  Language  │  │              │              │              │    │
│  Framework │  │   DESIGN     │     CODE     │   PREVIEW    │    │
│  Presets   │  │   INPUT      │    EDITOR    │    PANE      │    │
│            │  │              │              │              │    │
│            │  └──────────────┴──────────────┴──────────────┘    │
│            │                                                      │
├────────────┴────────────────────────────────────────────────────┤
│                         ACTION BAR                                │
│  [Settings] [Share] [Export]                                    │
└─────────────────────────────────────────────────────────────────┘
```

### Responsive Breakpoints

#### Mobile (< 640px)
- **Header**: Full width, sticky
- **Sidebar**: Hidden by default, slides from left when toggled
- **Main Content**: Single column, stacked vertically
  - Design Input (full width)
  - Code Editor (full width)
  - Preview (full width)
- **Action Bar**: Sticky at bottom

#### Tablet (640px - 1024px)
- **Header**: Full width, sticky
- **Sidebar**: Visible, collapsible
- **Main Content**: Two columns
  - Left: Design Input
  - Right: Code Editor + Preview (tabs)
- **Action Bar**: Sticky at bottom

#### Desktop (> 1024px)
- **Header**: Full width, sticky
- **Sidebar**: Visible, persistent
- **Main Content**: Three columns
  - Left: Design Input (1/3 width)
  - Center: Code Editor (1/3 width)
  - Right: Preview (1/3 width)
- **Action Bar**: Sticky at bottom

## Component Specifications

### Header Component

**Purpose**: Main navigation and theme control

**Dimensions**:
- Height: 4rem (64px)
- Sticky position
- z-index: 40

**Contents**:
```
┌─ Menu Button (mobile only)
├─ Logo Icon (8x8 rounded, "FC" text)
├─ Title "Figma to Code" (hidden on mobile)
└─ Theme Toggle
```

**CSS Classes**:
```
border-b border-border bg-background/95 backdrop-blur
sticky top-0 z-40
h-16 flex items-center gap-md px-md lg:px-lg
```

**States**:
- Normal: Light foreground on light background
- Dark: Dark background with light foreground
- Hover: Subtle background change on theme button
- Focus: 2px ring with offset

**Keyboard Navigation**:
- Tab through menu and theme buttons
- Enter/Space to activate
- Focus indicator always visible

### Sidebar Component

**Purpose**: Language and framework selection

**Dimensions**:
- Width: 16rem (256px)
- Full screen height minus header
- Fixed position on mobile, relative on desktop

**Contents**:
```
┌─ Language Selection
│  ├─ React
│  ├─ Vue
│  ├─ Svelte
│  └─ HTML
├─ Framework Selection
│  ├─ Tailwind
│  ├─ Styled
│  └─ CSS
├─ Quick Presets
│  ├─ React + Tailwind
│  └─ Vue + Tailwind
├─ Current Selection Display
└─ Spacer
```

**CSS Classes**:
```
fixed/relative top-16 left-0 z-40
w-64 h-[calc(100vh-4rem)]
border-r border-border bg-card
transform transition-transform duration-300
lg:translate-x-0 (always visible on desktop)
```

**Mobile Overlay**:
- Fixed inset-0 bg-black/50 when sidebar open
- z-index: 30
- Clicking overlay closes sidebar

**States**:
- Expanded: Full sidebar visible, overlay shown
- Collapsed: Sidebar off-screen, overlay hidden
- Active language/framework: Highlighted toggle state

### Design Input Panel

**Purpose**: Receive design files via upload or paste

**Dimensions**:
- Width: Full width (mobile), 1/3 (desktop)
- Min height: 400px
- Scrollable

**Contents**:
```
┌─ Upload Section
│  ├─ Drag & Drop Area
│  │  ├─ Upload Icon
│  │  ├─ "Drop your design here" text
│  │  └─ "or click to browse" hint
│  └─ File Input (hidden)
├─ Paste Section
│  ├─ Label "Or paste JSON"
│  ├─ Textarea (h-40)
│  └─ Placeholder text
├─ Action Buttons
│  ├─ Browse Button
│  └─ Paste Button
└─ Status Message (green when content loaded)
```

**CSS Classes**:
```
h-full flex flex-col gap-md p-md lg:p-lg
border-r border-border lg:border-r
```

**Drag & Drop States**:
- Default: `border-2 border-dashed border-border`
- Dragging: `border-primary bg-primary/5`
- Hover: `hover:border-primary/50 hover:bg-muted/50`

**Accessibility**:
- Drag area: `role="button" tabindex="0"`
- File input: `aria-label="File input for design upload"`
- Textarea: `aria-label="Paste design JSON"`
- Buttons: Descriptive aria-labels

### Code Editor Pane

**Purpose**: Display and manage generated code

**Dimensions**:
- Width: Full width (mobile), 1/3 (desktop)
- Min height: 400px
- Scrollable

**Contents**:
```
┌─ Header
│  ├─ Title "Code"
│  ├─ Copy Button
│  └─ Download Button
├─ Tabs
│  ├─ "Code" Tab
│  │  └─ Code Preview (monospace, dark background)
│  └─ "Dependencies" Tab
│     ├─ Required Packages
│     └─ Installation Command
└─ Content Area
   └─ Scrollable Code Display
```

**CSS Classes**:
```
h-full flex flex-col bg-card
border-r border-border lg:border-r
overflow-auto
```

**Code Display**:
- Font: Font mono, text-xs
- Padding: p-md lg:p-lg
- Color: text-muted-foreground
- Background: bg-muted (for code block)

**Tab States**:
- Active: `border-b-2 border-primary data-[state=active]`
- Inactive: `border-b-2 border-transparent`

**Copy Feedback**:
- Normal state: Copy icon
- After copy: "Copied!" tooltip appears for 2 seconds

### Preview Pane

**Purpose**: Display live component preview

**Dimensions**:
- Width: Full width (mobile), 1/3 (desktop)
- Min height: 400px
- Responsive iframe

**Contents**:
```
┌─ Header
│  ├─ Title "Preview"
│  ├─ Refresh Button
│  └─ Fullscreen Button
└─ Preview Area
   └─ iframe (sandbox, white background)
```

**CSS Classes**:
```
h-full flex flex-col bg-background
border-b lg:border-l border-border
```

**iframe Settings**:
- `sandbox="allow-scripts allow-same-origin"`
- `title="Preview"`
- White background (`style={{ backgroundColor: 'white' }}`)
- Full width/height

**Preview Content**:
- Auto-injects Tailwind via CDN
- Renders HTML/component output
- Responsive within iframe bounds

### Settings Drawer

**Purpose**: Configuration and preferences

**Trigger**: Gear icon in action bar

**Sheet Configuration**:
- Side: right
- Size: Default (410px on desktop, 3/4 on mobile)
- z-index: 50 (above sidebar)

**Contents**:
```
┌─ Header
│  ├─ "Settings & Configuration"
│  └─ "Customize your experience"
├─ Code Generation Settings
│  ├─ Optimize for performance (checkbox)
│  ├─ Include accessibility (checkbox)
│  └─ Generate TypeScript types (checkbox)
├─ Output Format
│  ├─ Component (.tsx) (radio)
│  └─ Inline HTML (radio)
├─ Spacing
│  └─ Base unit (px) (number input)
├─ Colors
│  ├─ Use CSS variables (checkbox)
│  └─ Include color palette (checkbox)
├─ Divider
└─ About
   └─ Version info
```

**CSS Classes**:
```
SheetContent: w-full sm:w-[410px]
SheetHeader: flex flex-col space-y-2
Internal sections: space-y-lg py-lg
Nested groups: space-y-md text-sm
```

**Close Interaction**:
- Click X button in top right
- Press Escape
- Click overlay

### Action Bar

**Purpose**: Export, share, and open settings

**Dimensions**:
- Height: auto (min 64px)
- Full width
- Sticky at bottom
- z-index: Default (not sticky)

**Contents**:
```
┌─ Status (hidden on small devices)
│  ├─ Zap Icon
│  └─ "Ready to export" text
├─ Spacer
└─ Actions (right-aligned)
   ├─ Settings Button (icon only)
   ├─ Share Button (icon + "Share" text on desktop)
   └─ Export Button (icon + "Export" text on desktop)
```

**CSS Classes**:
```
border-t border-border bg-card p-md lg:p-lg
flex items-center gap-md justify-between
```

**Button States**:
- Default: outline variant
- Export: primary variant
- Loading: disabled state with spinner text
- Hover: background change
- Focus: ring indicator

## Typography System

### Font Family
- Primary: Inter (loaded from Google Fonts)
- Monospace: Font mono (for code blocks)

### Font Sizes
```
h1: text-lg font-bold          (18px)
h2: text-sm font-semibold      (14px)
h3: text-sm font-semibold      (14px)
body: text-sm (14px)
labels: text-xs font-medium    (12px)
code: text-xs (12px)
```

### Font Weights
- Normal: 400
- Medium: 500
- Semibold: 600
- Bold: 700

### Line Heights
- Headings: 1.2
- Body: 1.5
- Code: 1.4

## Spacing System

### Base Units (Tailwind)
```
xs: 0.5rem   (8px)
sm: 0.75rem  (12px)
md: 1rem     (16px)
lg: 1.5rem   (24px)
xl: 2rem     (32px)
2xl: 3rem    (48px)
```

### Padding
- Container padding: `p-md lg:p-lg` (16px / 24px)
- Section padding: `p-md` (16px)
- Component internal: `p-sm` (12px)

### Gaps
- Large gaps: `gap-lg` (24px)
- Medium gaps: `gap-md` (16px)
- Small gaps: `gap-sm` (12px)
- Icon spacing: `gap-xs` (8px) - for icons next to text

## Color System

### Light Mode
- Background: #ffffff
- Foreground: #0a0a0a
- Muted: #f5f5f5
- Border: #e5e7eb
- Primary: #000000
- Accent: #171717

### Dark Mode
- Background: #0a0a0a
- Foreground: #fafafa
- Muted: #262626
- Border: #3d3d3d
- Primary: #fafafa
- Accent: #fafafa

### Semantic Colors
- Success: #10b981
- Warning: #f59e0b
- Error: #ef4444
- Info: #3b82f6

## Responsive Grid

### Mobile Layout (flex column)
```
Header
Sidebar (overlay)
├── Design Input (full width)
├── Code Editor (full width)
├── Preview (full width)
Action Bar
```

### Tablet Layout (lg:)
```
Header
├── Sidebar (25%)
├── Content (75%)
│  ├── Design Input (50%)
│  └── Code + Preview (50%, tabs)
Action Bar
```

### Desktop Layout (lg:)
```
Header
├── Sidebar (25%)
├── Design Input (25%)
├── Code Editor (25%)
├── Preview (25%)
Action Bar
```

## Reusable Patterns

### Button Variants
```tsx
// Primary action
<Button>Export</Button>

// Secondary action
<Button variant="outline">Share</Button>

// Icon button
<Button variant="ghost" size="icon">
  <Icon />
</Button>

// Icon + text
<Button variant="outline" size="sm">
  <Icon className="mr-xs" />
  <span className="hidden sm:inline">Text</span>
</Button>
```

### Focus States
```
outline: none;
ring: 2px ring-ring;
ring-offset: 2px ring-offset-background;

(dark mode: ring-offset-0)
```

### Hover States
- Buttons: `hover:bg-{color}/90`
- Ghost: `hover:bg-accent hover:text-accent-foreground`
- Outline: `hover:bg-accent hover:text-accent-foreground`

### Disabled States
- `disabled:pointer-events-none disabled:opacity-50`
- Cursor not-allowed
- Reduced opacity

## Animation & Transitions

### Theme Transition
```css
html {
  transition: background-color 0.3s ease, color 0.3s ease;
}
```

### Sidebar Mobile Transition
```
transform transition-transform duration-300 ease-in-out
translate-x-0 (open)
-translate-x-full (closed)
```

### Drawer Transition
```
data-[state=open]:animate-in
data-[state=closed]:animate-out
data-[state=open]:slide-in-from-right
duration-500
```

### Button Hover
```
transition-colors
duration-200
```

## Accessibility Features

### Focus Management
- All interactive elements have visible focus indicators
- Focus order follows visual layout
- Keyboard traps prevented
- Escape key closes modals/drawers

### ARIA Labels
- Icon buttons: `aria-label="description"`
- Toggle states: `aria-expanded={boolean}`
- Modals: `aria-modal="true"`
- Tabs: `role="tab"` and `aria-selected`

### Keyboard Shortcuts
- Tab: Navigate
- Shift+Tab: Navigate backward
- Enter/Space: Activate
- Escape: Close drawer
- Arrow keys: Toggle groups

### Color Contrast
- Text on background: 9.2:1 (light), 10:1 (dark)
- Interactive elements: ≥ 4.5:1

## Testing Checklist

### Visual Testing
- [ ] All panels visible at different breakpoints
- [ ] Colors match design system
- [ ] Typography hierarchy clear
- [ ] Spacing consistent
- [ ] Focus indicators visible

### Functional Testing
- [ ] File upload works
- [ ] Drag & drop works
- [ ] Paste textarea works
- [ ] Theme toggle works
- [ ] Sidebar toggle works
- [ ] Copy button works
- [ ] Download button works
- [ ] Settings drawer opens/closes
- [ ] All buttons are functional
- [ ] Links and navigation work

### Responsive Testing
- [ ] Mobile (320px, 480px)
- [ ] Tablet (768px, 1024px)
- [ ] Desktop (1280px+)
- [ ] Orientation changes (portrait/landscape)
- [ ] Zoom levels (100%, 150%, 200%)

### Accessibility Testing
- [ ] Keyboard navigation complete
- [ ] Screen reader test
- [ ] Focus order logical
- [ ] Focus indicators visible
- [ ] Color not sole indicator
- [ ] Touch targets ≥ 48px
- [ ] No keyboard trap

### Browser Testing
- [ ] Chrome/Edge
- [ ] Firefox
- [ ] Safari
- [ ] Mobile Safari
- [ ] Chrome Mobile

## Performance Considerations

### Code Splitting
- Components lazy-loaded when needed
- Routes code-split automatically

### Asset Optimization
- Icons: SVG (Lucide)
- Fonts: System fonts + Google Fonts
- Images: None in current implementation

### Bundle Size
- Target: < 100KB gzipped
- Dependencies optimized
- Tree-shaking enabled

### Runtime Performance
- Frame rate: 60 FPS
- First input delay: < 100ms
- Layout shift: 0

## Customization Guide

### Changing Colors
1. Edit `src/app/globals.css` (CSS variables)
2. Update `tailwind.config.ts` if needed
3. Verify contrast ratios
4. Test in both light and dark modes

### Changing Spacing
1. Edit `tailwind.config.ts` (spacing scale)
2. Update component classNames
3. Ensure consistency across app
4. Test responsive behavior

### Changing Typography
1. Edit font imports in `layout.tsx`
2. Update font scales in `tailwind.config.ts`
3. Update `globals.css` line heights
4. Test readability at different sizes

### Adding New Components
1. Create file in `src/components/`
2. Use existing UI primitives
3. Include accessibility features
4. Follow naming conventions
5. Export from layout if needed

---

This guide should help developers understand the layout structure and make consistent updates.
