'use client';

import React from 'react';
import useGeneratorStore from '@/stores/generatorStore';
import { PRESET_LIST } from '@/data/presets';

export function DesignInputPanel() {
  const { designInput, updateDesignInput } = useGeneratorStore();

  return (
    <div className="space-y-4 p-6 bg-white rounded-lg shadow-sm border border-gray-200">
      <h3 className="text-lg font-semibold text-gray-900">Design Input</h3>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Design Preset
        </label>
        <select
          value={designInput.preset}
          onChange={(e) => updateDesignInput({ preset: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        >
          {PRESET_LIST.map((preset) => (
            <option key={preset.id} value={preset.id}>
              {preset.name} - {preset.description}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Site Title
        </label>
        <input
          type="text"
          value={designInput.title}
          onChange={(e) => updateDesignInput({ title: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          placeholder="My Website"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Description
        </label>
        <textarea
          value={designInput.description}
          onChange={(e) => updateDesignInput({ description: e.target.value })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          placeholder="Describe your website..."
          rows={3}
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Layout
        </label>
        <select
          value={designInput.layout}
          onChange={(e) => updateDesignInput({ layout: e.target.value as any })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        >
          <option value="single-column">Single Column</option>
          <option value="two-column">Two Column</option>
          <option value="three-column">Three Column</option>
        </select>
      </div>

      <div className="space-y-3">
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={designInput.includeHero}
            onChange={(e) => updateDesignInput({ includeHero: e.target.checked })}
            className="w-4 h-4"
          />
          <span className="text-sm font-medium text-gray-700">Include Hero Section</span>
        </label>

        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={designInput.includeFeatures}
            onChange={(e) => updateDesignInput({ includeFeatures: e.target.checked })}
            className="w-4 h-4"
          />
          <span className="text-sm font-medium text-gray-700">Include Features Section</span>
        </label>

        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={designInput.includeFooter}
            onChange={(e) => updateDesignInput({ includeFooter: e.target.checked })}
            className="w-4 h-4"
          />
          <span className="text-sm font-medium text-gray-700">Include Footer</span>
        </label>
      </div>
    </div>
  );
}
