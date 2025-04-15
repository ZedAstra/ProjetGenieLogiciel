<script lang="ts">
	import '../app.css';
	import { AppBar, AppLayout } from 'svelte-ux';
	let { children } = $props();
</script>

<AppLayout areas="'header header' 'aside main'">
	<svelte:fragment slot="nav">
	  <!-- Nav menu -->
	</svelte:fragment>
  
	<AppBar title="App">
	  <div slot="actions">
		<!-- App actions -->
	  </div>
	</AppBar>
	<main class="min-h-full">
		{@render children()}
	</main>	
</AppLayout>