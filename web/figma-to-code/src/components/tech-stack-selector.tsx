'use client';

import React from 'react';
import useGeneratorStore from '@/stores/generatorStore';
import { LANGUAGE_LIST } from '@/data/languages';
import { FRAMEWORK_LIST } from '@/data/frameworks';
import type { Language, Framework } from '@/data/languages';

export function TechStackSelector() {
  const {
    selectedLanguages,
    selectedFramework,
    toggleLanguage,
    setFramework,
  } = useGeneratorStore();

  return (
    <div className="space-y-6 p-6 bg-white rounded-lg shadow-sm border border-gray-200">
      <div>
        <h3 className="text-lg font-semibold mb-4 text-gray-900">Languages</h3>
        <div className="grid grid-cols-2 gap-3">
          {LANGUAGE_LIST.map((language) => (
            <button
              key={language.id}
              onClick={() => toggleLanguage(language.id as Language)}
              className={`p-3 rounded-lg border-2 transition-all ${
                selectedLanguages.includes(language.id as Language)
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-200 bg-gray-50 hover:border-gray-300'
              }`}
            >
              <div className="font-medium text-sm text-gray-900">{language.name}</div>
              <div className="text-xs text-gray-500">{language.description}</div>
            </button>
          ))}
        </div>
      </div>

      <div className="border-t border-gray-200 pt-6">
        <h3 className="text-lg font-semibold mb-4 text-gray-900">Frameworks</h3>
        <div className="grid grid-cols-1 gap-3">
          {FRAMEWORK_LIST.map((framework) => (
            <button
              key={framework.id}
              onClick={() => setFramework(selectedFramework === framework.id as Framework ? null : framework.id as Framework)}
              className={`p-4 rounded-lg border-2 transition-all text-left ${
                selectedFramework === framework.id
                  ? 'border-green-500 bg-green-50'
                  : 'border-gray-200 bg-gray-50 hover:border-gray-300'
              }`}
            >
              <div className="font-medium text-gray-900">{framework.name}</div>
              <div className="text-sm text-gray-500">{framework.description}</div>
              <div className="text-xs text-gray-400 mt-2">v{framework.version}</div>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
