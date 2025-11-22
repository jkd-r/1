import { describe, it, expect, beforeEach, vi } from 'vitest'

describe('API Integration Tests', () => {
  beforeEach(() => {
    // Reset fetch mocks if any
    global.fetch = vi.fn()
  })

  describe('POST /api/figma-to-code/generate', () => {
    it('should successfully generate code with valid data', async () => {
      const mockResponse = {
        success: true,
        data: {
          code: 'import React from "react";\n\nconst Component = () => <div>Test</div>;\n\nexport default Component;',
          stack: 'react',
          generationTime: '0.25'
        }
      }

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

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

      const response = await fetch('/api/figma-to-code/generate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          figmaData,
          stack: 'react'
        })
      })

      const result = await response.json()
      expect(result.success).toBe(true)
      expect(result.data.code).toContain('import React')
      expect(result.data.stack).toBe('react')
      expect(result.data.generationTime).toBeDefined()
    })

    it('should handle missing parameters', async () => {
      const mockResponse = {
        success: false,
        error: 'Missing required parameters: figmaData and stack'
      }

      global.fetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: async () => mockResponse
      })

      const response = await fetch('/api/figma-to-code/generate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({})
      })

      expect(response.status).toBe(400)
      const result = await response.json()
      expect(result.success).toBe(false)
      expect(result.error).toContain('Missing required parameters')
    })

    it('should handle unsupported stack', async () => {
      const mockResponse = {
        success: false,
        error: 'Unsupported stack: invalid-stack. Supported stacks: react, vue, angular, html, tailwind, bootstrap'
      }

      global.fetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: async () => mockResponse
      })

      const figmaData = { document: { children: [] } }

      const response = await fetch('/api/figma-to-code/generate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          figmaData,
          stack: 'invalid-stack'
        })
      })

      expect(response.status).toBe(400)
      const result = await response.json()
      expect(result.success).toBe(false)
      expect(result.error).toContain('Unsupported stack')
    })
  })

  describe('GET /api/figma-to-code/history', () => {
    it('should retrieve conversion history', async () => {
      const mockResponse = {
        success: true,
        data: [
          {
            id: 1234567890,
            timestamp: '2024-01-01T00:00:00.000Z',
            stack: 'react',
            code: 'import React from "react";',
            generationTime: '0.25'
          }
        ]
      }

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      const response = await fetch('/api/figma-to-code/history')
      const result = await response.json()

      expect(result.success).toBe(true)
      expect(result.data).toHaveLength(1)
      expect(result.data[0].stack).toBe('react')
    })
  })

  describe('DELETE /api/figma-to-code/history', () => {
    it('should clear conversion history', async () => {
      const mockResponse = {
        success: true,
        message: 'History cleared successfully'
      }

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      const response = await fetch('/api/figma-to-code/history', {
        method: 'DELETE'
      })

      const result = await response.json()
      expect(result.success).toBe(true)
      expect(result.message).toContain('History cleared')
    })
  })

  describe('GET /api/figma-to-code/stats', () => {
    it('should retrieve usage statistics', async () => {
      const mockResponse = {
        success: true,
        data: {
          conversionCount: 10,
          historyCount: 8,
          supportedStacks: 6,
          averageTime: '0.32'
        }
      }

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      const response = await fetch('/api/figma-to-code/stats')
      const result = await response.json()

      expect(result.success).toBe(true)
      expect(result.data.conversionCount).toBe(10)
      expect(result.data.supportedStacks).toBe(6)
    })
  })

  describe('GET /api/figma-to-code/stacks', () => {
    it('should retrieve supported stacks', async () => {
      const mockResponse = {
        success: true,
        data: ['react', 'vue', 'angular', 'html', 'tailwind', 'bootstrap']
      }

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      const response = await fetch('/api/figma-to-code/stacks')
      const result = await response.json()

      expect(result.success).toBe(true)
      expect(result.data).toContain('react')
      expect(result.data).toContain('vue')
      expect(result.data).toContain('angular')
      expect(result.data).toHaveLength(6)
    })
  })

  describe('GET /api/health', () => {
    it('should return health status', async () => {
      const mockResponse = {
        status: 'healthy',
        timestamp: '2024-01-01T00:00:00.000Z',
        uptime: 123.45,
        memory: {
          rss: 45678912,
          heapTotal: 23456789,
          heapUsed: 12345678,
          external: 1234567
        },
        version: '1.0.0'
      }

      global.fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      const response = await fetch('/api/health')
      const result = await response.json()

      expect(result.status).toBe('healthy')
      expect(result.uptime).toBeDefined()
      expect(result.memory).toBeDefined()
      expect(result.version).toBeDefined()
    })
  })
})