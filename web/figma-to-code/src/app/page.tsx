'use client';

import React from 'react';
import { TechStackSelector } from '@/components/tech-stack-selector';
import { DesignInputPanel } from '@/components/design-input-panel';
import { CodeDisplay } from '@/components/code-display';

export default function Home() {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <h1 className="text-3xl font-bold text-gray-900">Figma to Code Generator</h1>
          <p className="text-gray-600 mt-1">
            Generate HTML, CSS, JavaScript, TypeScript, React, and Vue code from design inputs
          </p>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Sidebar - Inputs */}
          <div className="lg:col-span-1 space-y-6">
            <TechStackSelector />
            <DesignInputPanel />
          </div>

          {/* Main Area - Code Display */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h2 className="text-xl font-semibold mb-4 text-gray-900">Generated Code</h2>
              <CodeDisplay />
            </div>
          </div>
        </div>

        {/* Information Section */}
        <div className="mt-12 grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <h3 className="text-lg font-semibold mb-3 text-gray-900">How It Works</h3>
            <ul className="space-y-2 text-gray-600">
              <li className="flex gap-2">
                <span className="text-blue-500 font-bold">1.</span>
                <span>Select one or more languages (HTML, CSS, JS, TS, React, Vue)</span>
              </li>
              <li className="flex gap-2">
                <span className="text-blue-500 font-bold">2.</span>
                <span>Optionally choose a framework (Tailwind, React, Vue)</span>
              </li>
              <li className="flex gap-2">
                <span className="text-blue-500 font-bold">3.</span>
                <span>Configure your design with presets and options</span>
              </li>
              <li className="flex gap-2">
                <span className="text-blue-500 font-bold">4.</span>
                <span>Code is generated automatically with syntax validation</span>
              </li>
              <li className="flex gap-2">
                <span className="text-blue-500 font-bold">5.</span>
                <span>Copy and use the generated code in your project</span>
              </li>
            </ul>
          </div>

          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <h3 className="text-lg font-semibold mb-3 text-gray-900">Features</h3>
            <ul className="space-y-2 text-gray-600">
              <li className="flex gap-2">
                <span className="text-green-500">✓</span>
                <span>Multiple language support (HTML, CSS, JS, TS, React, Vue)</span>
              </li>
              <li className="flex gap-2">
                <span className="text-green-500">✓</span>
                <span>Framework integration (Tailwind, React, Vue)</span>
              </li>
              <li className="flex gap-2">
                <span className="text-green-500">✓</span>
                <span>Design presets (Minimal, Modern, Organic, Dark)</span>
              </li>
              <li className="flex gap-2">
                <span className="text-green-500">✓</span>
                <span>Real-time code generation and validation</span>
              </li>
              <li className="flex gap-2">
                <span className="text-green-500">✓</span>
                <span>Immediate updates on any setting change</span>
              </li>
              <li className="flex gap-2">
                <span className="text-green-500">✓</span>
                <span>One-click copy and easy download</span>
              </li>
            </ul>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-gray-900 text-gray-100 mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <p className="text-center text-gray-400">
            © 2024 Figma to Code Generator. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  );
}
