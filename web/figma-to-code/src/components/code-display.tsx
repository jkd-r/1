'use client';

import React, { useState } from 'react';
import useGeneratorStore from '@/stores/generatorStore';
import { LANGUAGE_LIST } from '@/data/languages';
import type { Language } from '@/data/languages';

export function CodeDisplay() {
  const { generatedCode, generatedFiles, validationErrors, selectedLanguages, selectedFramework } = useGeneratorStore();
  const [activeTab, setActiveTab] = useState<string>(selectedLanguages[0] || 'html');

  const displayItems = [
    ...selectedLanguages.map((lang) => ({
      id: lang,
      name: LANGUAGE_LIST.find((l) => l.id === lang)?.name || lang,
      code: generatedCode[lang],
      type: 'language' as const,
    })),
    ...generatedFiles.map((file) => ({
      id: file.name,
      name: file.name,
      code: file.content,
      type: 'file' as const,
    })),
  ];

  const currentItem = displayItems.find((item) => item.id === activeTab);
  const hasError = validationErrors[activeTab];

  if (displayItems.length === 0) {
    return (
      <div className="p-6 bg-gray-50 rounded-lg border border-gray-200 text-center">
        <p className="text-gray-500">Select a language or framework to generate code</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex gap-2 overflow-x-auto pb-2">
        {displayItems.map((item) => (
          <button
            key={item.id}
            onClick={() => setActiveTab(item.id)}
            className={`px-4 py-2 rounded-lg font-medium whitespace-nowrap transition-all ${
              activeTab === item.id
                ? 'bg-blue-500 text-white'
                : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
            }`}
          >
            {item.name}
            {hasError && activeTab === item.id && (
              <span className="ml-2 inline-block w-2 h-2 bg-red-500 rounded-full"></span>
            )}
          </button>
        ))}
      </div>

      {currentItem && (
        <div className="space-y-3">
          {validationErrors[activeTab] && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm font-medium text-red-900">Validation Error</p>
              <p className="text-sm text-red-700">{validationErrors[activeTab]}</p>
            </div>
          )}

          <div className="bg-gray-900 text-gray-100 rounded-lg p-4 overflow-auto max-h-96 font-mono text-sm">
            <pre>{currentItem.code}</pre>
          </div>

          <button
            onClick={() => {
              navigator.clipboard.writeText(currentItem.code);
            }}
            className="w-full px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
          >
            Copy Code
          </button>
        </div>
      )}
    </div>
  );
}
