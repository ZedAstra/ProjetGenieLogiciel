<script lang="ts">
	import '../app.css';
	import { AppBar, AppLayout } from 'svelte-ux';
	let { children } = $props();
</script>

<AppLayout>
	<svelte:fragment slot="nav">
	  <!-- Nav menu -->
	</svelte:fragment>
  
	<AppBar title="Example">
	  <div slot="actions">
		<!-- App actions -->
	  </div>
	</AppBar>
	<main>
		{@render children()}
	</main>	
</AppLayout>