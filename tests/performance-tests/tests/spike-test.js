/**
 * ============================================================================
 * SPIKE TEST
 * ============================================================================
 *
 * Перевіряє реакцію API на різке збільшення навантаження:
 * 1. Нормальний рівень: 100 VU протягом 2 хвилин.
 * 2. Сплеск: швидке збільшення до 2000 VU протягом 2 хвилин.
 * 3. Відновлення: швидке зниження до 0 VU протягом 1 хвилини.
 *
 */

import crudIteration from './crud-load-test.js';
import { DEFAULT_THRESHOLDS } from '../config/endpoints.js';

export const options = {
  stages: [
    { duration: '2m', target: 100 },
    { duration: '2m', target: 2000 },
    { duration: '1m', target: 0 },
  ],

  thresholds: {
    ...DEFAULT_THRESHOLDS,
    api_errors: ['rate==0'],
    checks: ['rate>0.95'],
    optimistic_lock_conflicts: ['rate>0'],
    'http_req_duration{type:read}': [
      'p(95)<500',
      'p(99)<1000',
    ],
    'http_req_duration{type:write}': [
      'p(95)<1000',
      'p(99)<2000',
    ],
  },

  noConnectionReuse: false,
  userAgent: 'K6-SpikeTest/1.0',
};

export default function () {
  crudIteration();
}

export function setup() {
  console.log('='.repeat(80));
  console.log('Starting CRUD spike test');
  console.log('Stages: 100 VUs for 2m → 2000 VUs for 2m → 0 VUs for 1m');
  console.log('Analyze request loss, API errors, latency and recovery after the spike');
  console.log('='.repeat(80));
}

export function teardown() {
  console.log('='.repeat(80));
  console.log('CRUD spike test completed');
  console.log('Compare p95/p99 latency, http_req_failed and api_errors during the spike');
  console.log('Recovery is successful when errors stop and latency returns to baseline');
  console.log('='.repeat(80));
}
