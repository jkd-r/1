# Protocol EMR Code Editor

A modern web-based code editor with live preview, code compilation, and project management features. Built with React, Monaco Editor, and Babel/Vue compiler.

## Features

### 🎨 Code Editing
- **Monaco Editor Integration**: Full-featured code editor with syntax highlighting and IntelliSense
- **Multi-Language Support**: JavaScript, TypeScript, JSX, TSX, Vue, HTML, and CSS
- **Dynamic Language Modes**: Switch between languages on the fly
- **File Management**: Create, edit, delete, and organize multiple files

### 👁️ Live Preview
- **Real-time Rendering**: Updates preview within <500ms of code changes
- **iframe Sandboxing**: Safe execution environment for user code
- **Multi-Framework Support**: 
  - React (via @babel/standalone)
  - Vue 3 (via Vue global build)
  - Vanilla JavaScript/HTML/CSS
- **Error Handling**: Displays compilation and runtime errors

### 💾 File Operations
- **Copy to Clipboard**: One-click copy of individual file contents
- **Batch Download**: Download all files as a ZIP archive using JSZip
- **Package.json Generation**: Auto-generates package.json if needed
- **README Generation**: Auto-generates README.md

### 📜 History & Persistence
- **Recent History Panel**: Lists last 50 code generations
- **localStorage Persistence**: 
  - Files survive page reload
  - Design inputs saved automatically
  - History persists across sessions
- **History Recovery**: Restore any previous state with a single click
- **Timestamps**: Relative time display (e.g., "5m ago")
- **Clear History**: Option to reset all history entries

### 🎯 Design Inputs
- **Component Naming**: Name your projects for easy identification
- **Description**: Add metadata to your components
- **History Restoration**: Restore design inputs with saved states

## Installation & Setup

### Prerequisites
- Node.js 16+ and npm/yarn/pnpm

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

### Development

The editor uses:
- **React 18**: UI framework
- **TypeScript**: Type safety
- **Vite**: Fast build tool and dev server
- **Monaco Editor (@monaco-editor/react)**: Code editing
- **@babel/standalone**: React/JSX compilation
- **vue**: Vue component support
- **JSZip**: File bundling and download

## Project Structure

```
src/
├── components/           # React components
│   ├── CodeEditor.tsx   # Monaco editor wrapper
│   ├── LivePreview.tsx  # iframe preview renderer
│   ├── FilePanel.tsx    # File management UI
│   ├── HistoryPanel.tsx # History viewer
│   ├── Toolbar.tsx      # Top toolbar
│   ├── Toast.tsx        # Notification system
│   └── ToastContainer.tsx
├── hooks/               # Custom React hooks
│   ├── useLocalStorage.ts  # localStorage persistence
│   └── useDebounce.ts      # Debounce hook
├── utils/              # Utility functions
│   ├── codeCompiler.ts  # Babel/Vue compilation
│   ├── clipboardUtils.ts # Copy to clipboard
│   └── downloadUtils.ts  # ZIP file generation
├── types.ts            # TypeScript types
├── App.tsx            # Main app component
└── main.tsx           # Entry point
```

## Usage

### Adding New Files

1. Click the **"+ New"** button in the Files panel
2. Or use the **"Add file by type"** dropdown menu
3. Select your desired language
4. Edit in the Monaco editor

### Editing Code

1. Select a file from the Files panel
2. Edit code in Monaco editor (left)
3. See live preview update in real-time (right)
4. Use the language dropdown to change syntax mode

### Copying Code

- Click the **📋** icon next to a file to copy its contents
- Toast notification confirms successful copy

### Downloading Files

1. Click **"⬇️ Download ZIP"** in the toolbar
2. Browser downloads `project-{timestamp}.zip`
3. Contains all files + auto-generated package.json and README.md

### Managing History

1. **Save**: Click **"💾 Save to History"** to save current state
2. **View**: History appears in the right panel with timestamps
3. **Restore**: Click any history entry to restore files and design inputs
4. **Clear**: Click **"Clear"** button to remove all history

## Performance

- **Compilation**: <500ms for code-to-preview updates (via debouncing)
- **Memory**: Efficient file caching and cleanup
- **Bundling**: Minimal dependencies, ~2MB initial bundle

## Browser Compatibility

- Chrome/Edge 90+
- Firefox 88+
- Safari 14+

Requires:
- ES2020 JavaScript support
- Clipboard API (with fallback for older browsers)
- localStorage API
- iframe support with sandbox attribute

## Limitations

### React/Vue Compilation
- **No build tools**: Uses CDN builds instead of bundlers
- **No npm packages**: Can't import external packages
- **Babel limitations**: Some advanced TypeScript features not supported
- **Vue SFC**: Simplified parsing, some advanced features not supported

### Preview Environment
- **Sandboxed**: Can't access parent window or APIs
- **No network requests**: CORS and external APIs limited
- **No local files**: Can't access filesystem

## Future Enhancements

- [ ] Export to GitHub Gists
- [ ] Code sharing with shareable URLs
- [ ] Syntax themes customization
- [ ] Keyboard shortcuts customization
- [ ] Collaborative editing (WebSocket)
- [ ] Terminal/console output in preview
- [ ] Multiple preview tabs
- [ ] Component previews library
- [ ] Built-in component templates
- [ ] Code formatting (Prettier integration)

## Development Workflow

### Adding a New Component

1. Create component file in `src/components/`
2. Create corresponding CSS file
3. Export from component file
4. Import and use in `App.tsx` or other components

### Adding a New Utility

1. Create utility file in `src/utils/`
2. Implement functions with proper TypeScript types
3. Import where needed

### Type Definitions

Add new types to `src/types.ts`:

```typescript
export interface MyType {
  // properties
}
```

## Troubleshooting

### Preview not updating
- Check browser console for errors
- Verify code has valid syntax for selected language
- Try changing language mode

### Copy not working
- Ensure browser supports Clipboard API
- Check browser console for permission errors
- Fallback to manual selection+copy

### Download failing
- Check browser console for errors
- Verify files have valid content
- Ensure sufficient disk space

### History not persisting
- Check localStorage is enabled in browser
- Verify localStorage quota not exceeded
- Check browser privacy/incognito mode

## License

Part of the Protocol EMR project.
