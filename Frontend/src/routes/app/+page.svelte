<script>
	import { Button } from "$lib/components/ui/button";
    import { getAppState, getJwtToken, getUserName } from "$lib/app.svelte";

    function getTimeAppropriateGreeting() {
        let date = new Date();
        let hours = date.getHours();
        if(hours < 12) return "Bonjour";
        if(hours < 18) return "Bon après-midi";
        return "Bonsoir";
    }

</script>

<div class="prose">
    <h1>{getTimeAppropriateGreeting() + " " + getUserName()}</h1>
</div>