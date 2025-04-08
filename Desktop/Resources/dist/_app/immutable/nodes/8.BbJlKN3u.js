import{a as v,t as h,c as ne,b as T}from"../chunks/CPxMcCbd.js";import{p as oe,w as se,a as ie,x as z,q as a,y as l,c as o,f as G,r as s,z as B,t as J}from"../chunks/DNSUiAPx.js";import{s as y}from"../chunks/DT3Q17oe.js";import{i as S}from"../chunks/BKeh4Tu3.js";import{e as le,i as de}from"../chunks/DJyUR5_m.js";import{p as $}from"../chunks/vI2wRnCw.js";import{B as C}from"../chunks/D4W2DxoK.js";import{a as j,b as L}from"../chunks/DqJa7-ml.js";import{I as k}from"../chunks/BqX89mNG.js";var ue=h('<div><p class="font-bold"> </p> <span class="inline-flex items-center gap-2"><!> <p> </p></span></div> <div class="flex flex-row"><!> <!></div>',1),ve=h('<div><p class="font-bold"> </p> <span class="inline-flex items-center gap-1"><p> </p> <p> </p></span></div>'),pe=h('<div class="flex flex-row justify-between items-center p-4 border-b"><div class="flex-1 flex flex-row justify-between"><!></div></div>'),ce=h('<div class="flex flex-row justify-center items-center p-4 border-b"><p>Aucune ressource disponible</p></div>'),me=h('<div class="flex flex-row justify-between items-center p-4 border-b"><!> <!> <!> <!></div>'),fe=h('<div class="flex flex-col"><div class="prose"><h1>Ressources</h1></div> <!> <!></div>');function je(Q,H){oe(H,!0);let p=se($([])),r=$({nom:"",quantité:0,unité:""});async function K(){var e=await j("/management/resources",{method:"GET",headers:{"Content-Type":"application/json"}});e.ok?z(p,$(await e.json())):console.error("Failed to fetch resources")}async function M(e){var t=await j("/management/resources/"+e.nom,{method:"DELETE"});t.ok?z(p,$(a(p).filter(i=>i.nom!==e.nom))):console.error("Failed to delete resource")}async function V(e){const t=document.getElementById("editBox-"+e.nom);let i={nom:e.nom,quantité:e.quantité,unité:e.unité};if(t)i.quantité=parseFloat(t.value);else{console.error("Input not found");return}let m=JSON.stringify(i);var n=await j("/management/resources/update",{method:"POST",body:m,headers:{"Content-Type":"application/json"}});if(n.ok){console.log("Resource updated successfully");for(let d=0;d<a(p).length;d++)if(a(p)[d].nom===e.nom){a(p)[d].quantité=i.quantité;break}}else console.error("Failed to update resource")}async function W(e){let t=new FormData;t.append("name",r.nom),t.append("quantity",r.quantité.toString()),t.append("unit",r.unité);var i=await j("/management/resources/add",{method:"POST",body:t});i.ok?a(p).push({nom:r.nom,quantité:r.quantité,unité:r.unité}):console.error("Failed to add resource"),r.nom="",r.quantité=0,r.unité=""}K();function I(e,...t){return t.includes(e)}var R=fe(),O=l(o(R),2);{var X=e=>{var t=ne(),i=G(t);le(i,17,()=>a(p),de,(m,n)=>{var d=pe(),u=o(d),D=o(u);{var F=f=>{var _=ue(),c=G(_),g=o(c),q=o(g,!0);s(g);var x=l(g,2),w=o(x);k(w,{get id(){return`editBox-${a(n).nom??""}`},class:"h-8",get value(){return a(n).quantité}});var b=l(w,2),P=o(b,!0);s(b),s(x),s(c);var N=l(c,2),U=o(N);C(U,{variant:"outline",class:"",onclick:()=>V(a(n)),children:(A,re)=>{B();var E=T("Enregistrer");v(A,E)},$$slots:{default:!0}});var ae=l(U,2);C(ae,{variant:"destructive",class:"",onclick:()=>M(a(n)),children:(A,re)=>{B();var E=T("Supprimer");v(A,E)},$$slots:{default:!0}}),s(N),J(()=>{y(q,a(n).nom),y(P,a(n).unité)}),v(f,_)},te=f=>{var _=ve(),c=o(_),g=o(c,!0);s(c);var q=l(c,2),x=o(q),w=o(x,!0);s(x);var b=l(x,2),P=o(b,!0);s(b),s(q),s(_),J(()=>{y(g,a(n).nom),y(w,a(n).quantité),y(P,a(n).unité)}),v(f,_)};S(D,f=>{I(L(),"Admin","Chef","Partenaire","Artisan")?f(F):f(te,!1)})}s(u),s(d),v(m,d)}),v(e,t)},Y=e=>{var t=ce();v(e,t)};S(O,e=>{a(p).length!==0?e(X):e(Y,!1)})}var Z=l(O,2);{var ee=e=>{var t=me(),i=o(t);k(i,{placeholder:"Nom de la ressource",get value(){return r.nom},set value(u){r.nom=u}});var m=l(i,2);k(m,{placeholder:"Quantité",type:"number",get value(){return r.quantité},set value(u){r.quantité=u}});var n=l(m,2);k(n,{placeholder:"Unité",get value(){return r.unité},set value(u){r.unité=u}});var d=l(n,2);C(d,{class:"ml-4",onclick:()=>W(),children:(u,D)=>{B();var F=T("Ajouter");v(u,F)},$$slots:{default:!0}}),s(t),v(e,t)};S(Z,e=>{I(L(),"Admin","Chef","Partenaire","Artisan")&&e(ee)})}s(R),v(Q,R),ie()}export{je as component};
