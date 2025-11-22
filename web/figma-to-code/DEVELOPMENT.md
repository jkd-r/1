# Development Guide

## Setting up the Development Environment

### Prerequisites
- Node.js 18.0.0 or higher
- npm or yarn
- Git

### Installation Steps

1. **Navigate to the web application directory**:
   ```bash
   cd web/figma-to-code
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Start the development server**:
   ```bash
   npm run dev
   ```

4. **Access the application**:
   Open your browser and navigate to `http://localhost:5173`

## Development Workflow

### Running Tests
```bash
# Run all tests
npm run test

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage

# Run performance-specific tests
npm run test:performance
```

### Performance Testing
```bash
# Run Lighthouse audit on development server
npm run lighthouse:dev

# Run automated Lighthouse CI
npm run lighthouse

# Check if scores meet >90 requirement
npm run lighthouse:check
```

### Building for Production
```bash
# Build the application
npm run build

# Preview production build
npm run preview

# Start production server
npm start
```

## Code Structure

The application follows a modular architecture:

- **`src/services/`**: Core business logic
  - `generator.js`: Main code generation engine
  - `generator.test.js`: Unit tests for generator
- **`src/controllers/`**: API request handlers
- **`src/routes/`**: Express.js route definitions
- **`src/integration/`**: Integration tests
- **`src/main.js`**: Frontend application logic
- **`scripts/`**: Utility scripts for testing and deployment

## Adding New Features

### Adding a New Tech Stack

1. **Update the generator** in `src/services/generator.js`:
   ```javascript
   convertToStack(figmaData, stack) {
     switch (stack) {
       case 'your-new-stack':
         return this.generateYourNewStackCode(figmaData)
       // ... existing cases
     }
   }
   
   getSupportedStacks() {
     return ['react', 'vue', 'angular', 'html', 'tailwind', 'bootstrap', 'your-new-stack']
   }
   ```

2. **Add tests** in `src/services/generator.test.js`:
   ```javascript
   test('should generate Your New Stack code successfully', () => {
     const result = generator.generateCode(sampleFigmaData, 'your-new-stack')
     expect(result.success).toBe(true)
     expect(result.code).toContain('your-new-stack-specific-code')
   })
   ```

3. **Update UI** in `index.html`:
   ```html
   <div class="stack-option" data-stack="your-new-stack">
     <h4>Your New Stack</h4>
     <p>Framework description</p>
   </div>
   ```

## Performance Guidelines

The application is optimized for >90 Lighthouse scores:

### Core Web Vitals Targets
- First Contentful Paint: <1.8s
- Largest Contentful Paint: <2.5s
- Cumulative Layout Shift: <0.1
- First Input Delay: <100ms

### Optimization Techniques
- Code splitting with Vite
- Tree shaking for dead code elimination
- Image optimization and lazy loading
- Critical CSS inlining
- Service worker for caching

## Testing Strategy

### Unit Tests
- Core generator functionality
- History serialization
- Stack-specific code generation
- Error handling

### Integration Tests
- API endpoint testing
- Frontend-backend integration
- Error scenarios

### Performance Tests
- Lighthouse score validation
- Bundle size monitoring
- Core Web Vitals measurement

## Deployment

### Environment Setup
1. **Set environment variables**:
   ```bash
   NODE_ENV=production
   PORT=3000
   ```

2. **Build the application**:
   ```bash
   npm run build
   ```

3. **Start production server**:
   ```bash
   npm start
   ```

### Docker Deployment
```dockerfile
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build
EXPOSE 3000
CMD ["npm", "start"]
```

## Troubleshooting

### Common Issues

1. **Tests failing**:
   - Check that all dependencies are installed
   - Ensure Node.js version is 18+
   - Clear cache: `rm -rf node_modules/.cache`

2. **Lighthouse scores low**:
   - Run `npm run lighthouse:dev` for detailed report
   - Check bundle size in build output
   - Review Core Web Vitals in browser DevTools

3. **Build failures**:
   - Check for TypeScript errors
   - Verify all imports are correct
   - Review build logs for specific errors

### Debug Mode
Enable debug logging:
```javascript
localStorage.setItem('debug', 'figma-to-code:*')
```

## Contributing

### Code Standards
- Use ES6+ features
- Follow existing naming conventions
- Add tests for new features
- Maintain >90% test coverage
- Ensure >90 Lighthouse scores

### Pull Request Process
1. Create feature branch
2. Implement changes with tests
3. Run `npm run test` and `npm run lighthouse:check`
4. Submit pull request with description

## Monitoring

### Performance Monitoring
- Lighthouse CI integration
- Core Web Vitals tracking
- Bundle size monitoring
- Error tracking

### Health Checks
- `/api/health` endpoint
- Memory usage monitoring
- Uptime tracking
- Performance metrics