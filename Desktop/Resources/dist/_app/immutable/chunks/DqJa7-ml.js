import{w as f,q as a,x as E}from"./DNSUiAPx.js";import{p as d}from"./vI2wRnCw.js";import{g as h}from"./DYcHq17l.js";new TextEncoder;const I=new TextDecoder,p=t=>{const e=atob(t),r=new Uint8Array(e.length);for(let o=0;o<e.length;o++)r[o]=e.charCodeAt(o);return r},J=t=>{let e=t;e instanceof Uint8Array&&(e=I.decode(e)),e=e.replace(/-/g,"+").replace(/_/g,"/").replace(/\s/g,"");try{return p(e)}catch{throw new TypeError("The input to be decoded is not correctly encoded.")}};class s extends Error{constructor(e,r){var o;super(e,r),this.code="ERR_JOSE_GENERIC",this.name=this.constructor.name,(o=Error.captureStackTrace)==null||o.call(Error,this,this.constructor)}}s.code="ERR_JOSE_GENERIC";class S extends s{constructor(e,r,o="unspecified",n="unspecified"){super(e,{cause:{claim:o,reason:n,payload:r}}),this.code="ERR_JWT_CLAIM_VALIDATION_FAILED",this.claim=o,this.reason=n,this.payload=r}}S.code="ERR_JWT_CLAIM_VALIDATION_FAILED";class g extends s{constructor(e,r,o="unspecified",n="unspecified"){super(e,{cause:{claim:o,reason:n,payload:r}}),this.code="ERR_JWT_EXPIRED",this.claim=o,this.reason=n,this.payload=r}}g.code="ERR_JWT_EXPIRED";class W extends s{constructor(){super(...arguments),this.code="ERR_JOSE_ALG_NOT_ALLOWED"}}W.code="ERR_JOSE_ALG_NOT_ALLOWED";class T extends s{constructor(){super(...arguments),this.code="ERR_JOSE_NOT_SUPPORTED"}}T.code="ERR_JOSE_NOT_SUPPORTED";class O extends s{constructor(e="decryption operation failed",r){super(e,r),this.code="ERR_JWE_DECRYPTION_FAILED"}}O.code="ERR_JWE_DECRYPTION_FAILED";class A extends s{constructor(){super(...arguments),this.code="ERR_JWE_INVALID"}}A.code="ERR_JWE_INVALID";class N extends s{constructor(){super(...arguments),this.code="ERR_JWS_INVALID"}}N.code="ERR_JWS_INVALID";class c extends s{constructor(){super(...arguments),this.code="ERR_JWT_INVALID"}}c.code="ERR_JWT_INVALID";class w extends s{constructor(){super(...arguments),this.code="ERR_JWK_INVALID"}}w.code="ERR_JWK_INVALID";class m extends s{constructor(){super(...arguments),this.code="ERR_JWKS_INVALID"}}m.code="ERR_JWKS_INVALID";class L extends s{constructor(e="no applicable key found in the JSON Web Key Set",r){super(e,r),this.code="ERR_JWKS_NO_MATCHING_KEY"}}L.code="ERR_JWKS_NO_MATCHING_KEY";class y extends s{constructor(e="multiple matching keys found in the JSON Web Key Set",r){super(e,r),this.code="ERR_JWKS_MULTIPLE_MATCHING_KEYS"}}y.code="ERR_JWKS_MULTIPLE_MATCHING_KEYS";class D extends s{constructor(e="request timed out",r){super(e,r),this.code="ERR_JWKS_TIMEOUT"}}D.code="ERR_JWKS_TIMEOUT";class b extends s{constructor(e="signature verification failed",r){super(e,r),this.code="ERR_JWS_SIGNATURE_VERIFICATION_FAILED"}}b.code="ERR_JWS_SIGNATURE_VERIFICATION_FAILED";function x(t){return typeof t=="object"&&t!==null}function K(t){if(!x(t)||Object.prototype.toString.call(t)!=="[object Object]")return!1;if(Object.getPrototypeOf(t)===null)return!0;let e=t;for(;Object.getPrototypeOf(e)!==null;)e=Object.getPrototypeOf(e);return Object.getPrototypeOf(t)===e}const j=J;function u(t){if(typeof t!="string")throw new c("JWTs must use Compact JWS serialization, JWT must be a string");const{1:e,length:r}=t.split(".");if(r===5)throw new c("Only JWTs using Compact JWS serialization can be decoded");if(r!==3)throw new c("Invalid JWT");if(!e)throw new c("JWTs must contain a payload");let o;try{o=j(e)}catch{throw new c("Failed to base64url decode the payload")}let n;try{n=JSON.parse(I.decode(o))}catch{throw new c("Failed to parse the decoded payload as JSON")}if(!K(n))throw new c("Invalid JWT Claims Set");return n}let i=f(d({jwtString:"",userId:"",isLoggedIn:!1}));function l(){return!a(i).isLoggedIn&&a(i).jwtString==""&&C(),a(i)}function _(){return l().jwtString}function M(t){R(t)&&(l().jwtString=t)}function R(t){const e=u(t);return e.exp&&e.exp<Date.now()/1e3?!1:(localStorage.setItem("jwt",t),E(i,d({jwtString:t,userId:e.sub,isLoggedIn:!0})),!0)}function U(){return l().userId}function v(){var t=_();if(t==null)return"None";const e=u(t);if(e.role){if(e.role.includes("Admin"))return"Admin";if(e.role.includes("Chef"))return"Chef";if(e.role.includes("Partenaire"))return"Partenaire";if(e.role.includes("Artisan"))return"Artisan";if(e.role.includes("Ouvrier"))return"Ouvrier"}return"None"}function G(){var t=_();if(t==null)return"";const e=u(t);return e.name?e.name:""}function k(t,e){return e.headers==null&&(e.headers={}),t.startsWith("/")&&(t=t.substring(1)),t!="auth/login"&&(e.headers.Authorization="Bearer "+_()),fetch(`https://localhost:7149/${t}`,e)}function C(){const t=localStorage.getItem("jwt");t&&R(t)}function Y(){localStorage.removeItem("jwt"),E(i,d({jwtString:"",userId:"",isLoggedIn:!1})),h("/")}export{k as a,v as b,l as c,U as d,G as g,Y as l,M as s};
