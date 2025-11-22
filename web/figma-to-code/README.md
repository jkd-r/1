# Figma to Code Generator

> A powerful web application that converts Figma designs to production-ready code

## 🚀 Features

- **Multi-Framework Support**: React, Vue, Angular, HTML, Tailwind CSS, Bootstrap
- **Real-time Conversion**: Instant code generation with performance metrics
- **History Management**: Track and reload previous conversions
- **Live Preview**: Preview HTML output in real-time
- **Performance Optimized**: >90 Lighthouse score target

## 🛠️ Quick Start

```bash
# Clone and navigate to project
cd web/figma-to-code

# Install dependencies
npm install

# Start development server
npm run dev

# Open browser to http://localhost:5173
```

## 📋 Available Scripts

```bash
npm run dev          # Start development server
npm run build        # Build for production
npm run test         # Run Vitest tests
npm run test:ui      # Run tests with UI
npm run test:coverage # Run tests with coverage
npm run lighthouse    # Run Lighthouse CI audit
npm run lighthouse:dev # Run Lighthouse on dev server
npm start            # Start production server
```

## 🎯 Performance

The application is optimized for >90 Lighthouse scores:

- **Performance**: >90
- **Accessibility**: >90  
- **Best Practices**: >90
- **SEO**: >90

Run `npm run lighthouse:dev` to verify performance.

## 📖 Documentation

- **[README.md](./README.md)**: Complete documentation
- **[QUICK_START.md](./QUICK_START.md)**: 5-minute setup guide
- **[DEVELOPMENT.md](./DEVELOPMENT.md)**: Development setup
- **[LIGHTHOUSE_CHECKLIST.md](./LIGHTHOUSE_CHECKLIST.md)**: Performance optimization

## 🧪 Testing

The application includes comprehensive testing:

- **Unit Tests**: Core generator functionality
- **Integration Tests**: API endpoints
- **History Tests**: Data persistence validation
- **Performance Tests**: Lighthouse score validation

```bash
# Run all tests
npm run test

# Run specific test suites
npm run test:performance
npm run lighthouse:check
```

## 🏗️ Architecture

```
web/figma-to-code/
├── src/
│   ├── services/      # Core business logic
│   ├── controllers/   # API handlers
│   ├── routes/        # Express routes
│   ├── integration/   # API tests
│   └── main.js       # Frontend app
├── scripts/          # Utility scripts
├── public/           # Static assets
└── dist/            # Build output
```

## 🎨 Supported Tech Stacks

| Stack | Features |
|-------|----------|
| **React** | JSX, hooks, functional components |
| **Vue 3** | Composition API, Single File Components |
| **Angular** | TypeScript, dependency injection |
| **HTML/CSS** | Semantic markup, responsive design |
| **Tailwind CSS** | Utility-first CSS framework |
| **Bootstrap** | Grid system, components |

## 🌟 Getting Help

- **Issues**: Report bugs and feature requests
- **Documentation**: Check inline comments and docs
- **Performance**: Run Lighthouse audits for insights

## 📄 License

MIT License - see [LICENSE](./LICENSE) for details.

---

Built with ❤️ for the design and development community