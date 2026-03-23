import http from 'k6/http';
import { Counter } from 'k6/metrics';
import { check } from 'k6';

const allowedRequests = new Counter('rate_limit_allowed');
const blockedRequests = new Counter('rate_limit_blocked');

export const options = {
  stages: [
    { duration: '20s', target: 200 }, 
    { duration: '30s', target: 500 }, 
    { duration: '10s', target: 0 },   
  ],
  thresholds: {
    http_req_failed: ['rate<0.999'],
  },
};

export default function () {
  const res = http.get('http://localhost:5068/');

  if (res.status === 200) {
    allowedRequests.add(1);
  } else if (res.status === 429) {
    blockedRequests.add(1);
  }

  check(res, {
    'is not 500': (r) => r.status < 500,
  });

}
