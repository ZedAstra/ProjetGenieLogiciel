<script lang="ts">
    import '../app.css';
    import { getSettings, Settings } from 'svelte-ux';

    let { children } = $props();
    getSettings().currentTheme.setTheme("light");
</script>

{@render children()}
<Settings />