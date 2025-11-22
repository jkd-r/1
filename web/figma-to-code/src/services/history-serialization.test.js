import { describe, it, expect, beforeEach, afterEach } from 'vitest'
import { vi } from 'vitest'
import { JSDOM } from 'jsdom'

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}
global.localStorage = localStorageMock

// Mock performance
global.performance = {
  now: vi.fn(() => Date.now())
}

describe('History Serialization', () => {
  beforeEach(() => {
    localStorageMock.getItem.mockClear()
    localStorageMock.setItem.mockClear()
  })

  test('should serialize history to JSON correctly', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const sampleHistory = [
      {
        id: 1234567890,
        timestamp: '2024-01-01T00:00:00.000Z',
        stack: 'react',
        figmaData: { document: { children: [] } },
        code: 'import React from "react";',
        generationTime: '0.25'
      }
    ]

    // Mock localStorage to return our sample history
    localStorageMock.getItem.mockReturnValue(JSON.stringify(sampleHistory))

    const history = generator.loadHistory()
    expect(history).toEqual(sampleHistory)
    expect(localStorageMock.getItem).toHaveBeenCalledWith('figmaToCodeHistory')
  })

  test('should handle corrupted history gracefully', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    // Mock localStorage to return invalid JSON
    localStorageMock.getItem.mockReturnValue('invalid json')

    const history = generator.loadHistory()
    expect(history).toEqual([])
  })

  test('should save history to localStorage correctly', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const historyItem = {
      id: 1234567890,
      timestamp: '2024-01-01T00:00:00.000Z',
      stack: 'react',
      figmaData: { document: { children: [] } },
      code: 'import React from "react";',
      generationTime: '0.25'
    }

    generator.addToHistory(historyItem)

    expect(localStorageMock.setItem).toHaveBeenCalledWith(
      'figmaToCodeHistory',
      expect.stringContaining('"id":1234567890')
    )
  })

  test('should limit history size to 50 items', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    // Start with empty history
    localStorageMock.getItem.mockReturnValue('[]')

    // Add 60 items
    for (let i = 0; i < 60; i++) {
      generator.addToHistory({
        id: i,
        timestamp: new Date().toISOString(),
        stack: 'react',
        figmaData: { document: { children: [] } },
        code: `// Code ${i}`,
        generationTime: '0.25'
      })
    }

    // Check that only last call contains exactly 50 items
    const lastCall = localStorageMock.setItem.mock.calls[localStorageMock.setItem.mock.calls.length - 1]
    const savedHistory = JSON.parse(lastCall[1])
    expect(savedHistory).toHaveLength(50)
  })

  test('should preserve history item structure', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    localStorageMock.getItem.mockReturnValue('[]')

    const historyItem = {
      id: Date.now(),
      timestamp: new Date().toISOString(),
      stack: 'vue',
      figmaData: { 
        document: { 
          children: [
            { name: 'Test Component', type: 'FRAME' }
          ]
        } 
      },
      code: '<template><div>Test</div></template>',
      generationTime: '0.15'
    }

    generator.addToHistory(historyItem)

    expect(localStorageMock.setItem).toHaveBeenCalledWith(
      'figmaToCodeHistory',
      expect.stringMatching(new RegExp(`"id":${historyItem.id}`))
    )

    const savedCall = localStorageMock.setItem.mock.calls.find(call => 
      call[0] === 'figmaToCodeHistory'
    )
    const savedHistory = JSON.parse(savedCall[1])
    
    expect(savedHistory[0]).toMatchObject({
      id: historyItem.id,
      timestamp: historyItem.timestamp,
      stack: historyItem.stack,
      code: historyItem.code,
      generationTime: historyItem.generationTime
    })
    expect(savedHistory[0]).toHaveProperty('figmaData')
  })

  test('should handle localStorage quota exceeded', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    localStorageMock.getItem.mockReturnValue('[]')
    
    // Mock localStorage.setItem to throw quota exceeded error
    localStorageMock.setItem.mockImplementation(() => {
      throw new Error('QuotaExceededError')
    })

    // Should not throw, but should log warning
    expect(() => {
      generator.addToHistory({
        id: Date.now(),
        timestamp: new Date().toISOString(),
        stack: 'react',
        figmaData: { document: { children: [] } },
        code: '// Test code',
        generationTime: '0.25'
      })
    }).not.toThrow()
  })
})

describe('Code Generation Validation', () => {
  test('should generate syntactically valid React code', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const figmaData = {
      document: {
        children: [
          {
            id: '1:1',
            name: 'Button',
            type: 'RECTANGLE',
            visible: true,
            absoluteBoundingBox: { x: 0, y: 0, width: 100, height: 40 },
            fills: [{ type: 'SOLID', visible: true, color: { r: 0.2, g: 0.4, b: 0.8 } }],
            cornerRadius: 4,
            children: []
          }
        ]
      }
    }

    const result = generator.generateCode(figmaData, 'react')
    expect(result.success).toBe(true)

    // Basic React component structure validation
    expect(result.code).toMatch(/import\s+React/)
    expect(result.code).toMatch(/const\s+\w+\s*=\s*\(\s*\)\s*=>/)
    expect(result.code).toMatch(/return\s*\(/)
    expect(result.code).toMatch(/export\s+default/)
    
    // JSX validation
    expect(result.code).toMatch(/<div/)
    expect(result.code).match(/<\/div>/)
    expect(result.code).toMatch(/style=\{/)
  })

  test('should generate syntactically valid Vue code', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const figmaData = {
      document: {
        children: [
          {
            id: '1:1',
            name: 'Card',
            type: 'FRAME',
            visible: true,
            absoluteBoundingBox: { x: 0, y: 0, width: 200, height: 150 },
            fills: [{ type: 'SOLID', visible: true, color: { r: 1, g: 1, b: 1 } }],
            children: []
          }
        ]
      }
    }

    const result = generator.generateCode(figmaData, 'vue')
    expect(result.success).toBe(true)

    // Vue SFC structure validation
    expect(result.code).toContain('<template>')
    expect(result.code).toContain('</template>')
    expect(result.code).toContain('<script setup>')
    expect(result.code).toContain('</script>')
    expect(result.code).toContain('<style scoped>')
    expect(result.code).toContain('</style>')
  })

  test('should generate syntactically valid Angular code', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const figmaData = {
      document: {
        children: [
          {
            id: '1:1',
            name: 'Component',
            type: 'FRAME',
            visible: true,
            absoluteBoundingBox: { x: 0, y: 0, width: 150, height: 100 },
            fills: [{ type: 'SOLID', visible: true, color: { r: 0.9, g: 0.9, b: 0.9 } }],
            children: []
          }
        ]
      }
    }

    const result = generator.generateCode(figmaData, 'angular')
    expect(result.success).toBe(true)

    // Angular component structure validation
    expect(result.code).toContain('import { Component }')
    expect(result.code).toContain('@Component({')
    expect(result.code).toContain('selector:')
    expect(result.code).toContain('template:')
    expect(result.code).toContain('export class')
  })

  test('should generate valid HTML document', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const figmaData = {
      document: {
        children: [
          {
            id: '1:1',
            name: 'Container',
            type: 'FRAME',
            visible: true,
            absoluteBoundingBox: { x: 0, y: 0, width: 300, height: 200 },
            fills: [{ type: 'SOLID', visible: true, color: { r: 1, g: 1, b: 1 } }],
            children: []
          }
        ]
      }
    }

    const result = generator.generateCode(figmaData, 'html')
    expect(result.success).toBe(true)

    // HTML document structure validation
    expect(result.code).toContain('<!DOCTYPE html>')
    expect(result.code).toContain('<html lang="en">')
    expect(result.code).toContain('<head>')
    expect(result.code).toContain('<meta charset="UTF-8">')
    expect(result.code).toContain('<meta name="viewport"')
    expect(result.code).toContain('<title>')
    expect(result.code).toContain('<body>')
    expect(result.code).toContain('</html>')
  })

  test('should generate non-empty code for all stacks', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const figmaData = {
      document: {
        children: [
          {
            id: '1:1',
            name: 'Test',
            type: 'RECTANGLE',
            visible: true,
            absoluteBoundingBox: { x: 0, y: 0, width: 100, height: 100 },
            fills: [{ type: 'SOLID', visible: true, color: { r: 0.5, g: 0.5, b: 0.5 } }],
            children: []
          }
        ]
      }
    }

    const stacks = generator.getSupportedStacks()
    
    stacks.forEach(stack => {
      const result = generator.generateCode(figmaData, stack)
      expect(result.success).toBe(true)
      expect(result.code).toBeTruthy()
      expect(result.code.length).toBeGreaterThan(0)
      
      // Each stack should have some distinctive content
      if (stack === 'react' || stack === 'tailwind' || stack === 'bootstrap') {
        expect(result.code).toContain('import React')
      }
      if (stack === 'vue') {
        expect(result.code).toContain('<template>')
      }
      if (stack === 'angular') {
        expect(result.code).toContain('@Component')
      }
      if (stack === 'html') {
        expect(result.code).toContain('<!DOCTYPE html>')
      }
    })
  })

  test('should handle complex nested structures', async () => {
    const { FigmaToCodeGenerator } = await import('./generator.js')
    const generator = new FigmaToCodeGenerator()

    const complexFigmaData = {
      document: {
        children: [
          {
            id: '1:1',
            name: 'Main Container',
            type: 'FRAME',
            visible: true,
            absoluteBoundingBox: { x: 0, y: 0, width: 400, height: 300 },
            fills: [{ type: 'SOLID', visible: true, color: { r: 1, g: 1, b: 1 } }],
            children: [
              {
                id: '2:1',
                name: 'Header',
                type: 'FRAME',
                visible: true,
                absoluteBoundingBox: { x: 10, y: 10, width: 380, height: 60 },
                fills: [{ type: 'SOLID', visible: true, color: { r: 0.9, g: 0.9, b: 0.9 } }],
                children: []
              },
              {
                id: '2:2',
                name: 'Content',
                type: 'FRAME',
                visible: true,
                absoluteBoundingBox: { x: 10, y: 80, width: 380, height: 150 },
                fills: [{ type: 'SOLID', visible: true, color: { r: 0.95, g: 0.95, b: 0.95 } }],
                children: [
                  {
                    id: '3:1',
                    name: 'Button',
                    type: 'RECTANGLE',
                    visible: true,
                    absoluteBoundingBox: { x: 20, y: 20, width: 100, height: 40 },
                    fills: [{ type: 'SOLID', visible: true, color: { r: 0.2, g: 0.6, b: 1 } }],
                    cornerRadius: 6,
                    children: []
                  }
                ]
              }
            ]
          }
        ]
      }
    }

    const result = generator.generateCode(complexFigmaData, 'react')
    expect(result.success).toBe(true)
    expect(result.code.length).toBeGreaterThan(500) // Should be substantial for nested structure
    
    // Should contain multiple div elements for nested structure
    const divCount = (result.code.match(/<div/g) || []).length
    expect(divCount).toBeGreaterThan(3)
  })
})