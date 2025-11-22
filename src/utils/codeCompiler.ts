import * as Babel from '@babel/standalone'
import { LanguageMode } from '../types'

export async function compileReactCode(code: string): Promise<string> {
  try {
    const result = Babel.transform(code, {
      presets: ['react', 'typescript'],
      filename: 'component.jsx',
      retainLines: true,
    })
    return result.code
  } catch (error) {
    throw new Error(`React compilation failed: ${error instanceof Error ? error.message : String(error)}`)
  }
}

export async function compileVueCode(code: string): Promise<string> {
  // Vue compilation would require @vue/compiler-sfc which is heavier
  // For now, we'll create a simplified HTML wrapper
  try {
    const templateMatch = code.match(/<template>([\s\S]*?)<\/template>/)
    const scriptMatch = code.match(/<script>([\s\S]*?)<\/script>/)
    const styleMatch = code.match(/<style>([\s\S]*?)<\/style>/)

    const template = templateMatch ? templateMatch[1].trim() : ''
    const script = scriptMatch ? scriptMatch[1].trim() : ''
    const style = styleMatch ? styleMatch[1].trim() : ''

    // Return wrapped Vue component
    return `
      <div id="app">
        ${template}
      </div>
      <style>
        ${style}
      </style>
      <script type="module">
        import { createApp } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js'
        ${script}
      </script>
    `
  } catch (error) {
    throw new Error(`Vue compilation failed: ${error instanceof Error ? error.message : String(error)}`)
  }
}

export function wrapInHtmlTemplate(content: string, language: LanguageMode): string {
  const style = `
    body {
      margin: 0;
      padding: 20px;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif;
      background-color: #f5f5f5;
    }
    #root {
      background: white;
      border-radius: 8px;
      padding: 20px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }
  `

  if (language === 'html') {
    return content
  }

  if (language === 'jsx' || language === 'tsx') {
    return `
      <!DOCTYPE html>
      <html lang="en">
      <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>React Preview</title>
        <style>${style}</style>
      </head>
      <body>
        <div id="root"></div>
        <script crossorigin src="https://unpkg.com/react@18/umd/react.production.min.js"></script>
        <script crossorigin src="https://unpkg.com/react-dom@18/umd/react-dom.production.min.js"></script>
        <script src="https://unpkg.com/@babel/standalone/babel.min.js"></script>
        <script type="text/babel">
          ${content}
        </script>
      </body>
      </html>
    `
  }

  if (language === 'vue') {
    return `
      <!DOCTYPE html>
      <html lang="en">
      <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Vue Preview</title>
        <style>${style}</style>
      </head>
      <body>
        <div id="root"></div>
        <script src="https://unpkg.com/vue@3/dist/vue.global.js"></script>
        <script type="text/babel">
          ${content}
        </script>
      </body>
      </html>
    `
  }

  if (language === 'javascript' || language === 'typescript') {
    return `
      <!DOCTYPE html>
      <html lang="en">
      <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>JavaScript Preview</title>
        <style>${style}</style>
      </head>
      <body>
        <div id="root"></div>
        <script>
          ${content}
        </script>
      </body>
      </html>
    `
  }

  if (language === 'css') {
    return `
      <!DOCTYPE html>
      <html lang="en">
      <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>CSS Preview</title>
        <style>
          ${content}
        </style>
      </head>
      <body>
        <div class="container">
          <h1>CSS Preview</h1>
          <p>Your CSS is now active</p>
        </div>
      </body>
      </html>
    `
  }

  return content
}

export async function compileCode(code: string, language: LanguageMode): Promise<string> {
  try {
    if (language === 'jsx' || language === 'tsx') {
      const compiled = await compileReactCode(code)
      return wrapInHtmlTemplate(compiled, language)
    }

    if (language === 'vue') {
      return await compileVueCode(code)
    }

    return wrapInHtmlTemplate(code, language)
  } catch (error) {
    console.error('Compilation error:', error)
    throw error
  }
}
