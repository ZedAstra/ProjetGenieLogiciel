<script lang="ts">
    import Box from "lucide-svelte/icons/box";
    import Calendar from "lucide-svelte/icons/calendar";
    import House from "lucide-svelte/icons/house";
    import MessageCircle from "lucide-svelte/icons/message-circle";
    import Newspaper from "lucide-svelte/icons/newspaper";
    import Settings from "lucide-svelte/icons/settings";
    import * as Sidebar from "$lib/components/ui/sidebar/index.js";
   
    // Menu items.
    const items = [
      {
        title: "Acceuil",
        url: "/app",
        icon: House,
      },
      // {
      //   title: "Chat",
      //   url: "/app/chat",
      //   icon: MessageCircle,
      // },
      {
        title: "Planning",
        url: "/app/planning",
        icon: Calendar,
      },
      {
        title: "Communication",
        url: "/app/communication",
        icon: Newspaper,
      },
      {
        title: "Ressources",
        url: "/app/resources",
        icon: Box,
      },
      /*{
        title: "Paramètres",
        url: "/app/settings",
        icon: Settings,
      },*/
    ];
  </script>
   
  <Sidebar.Root>
    <Sidebar.Content>
      <Sidebar.Group>
        <Sidebar.GroupLabel>Application</Sidebar.GroupLabel>
        <Sidebar.GroupContent>
          <Sidebar.Menu>
            {#each items as item (item.title)}
              <Sidebar.MenuItem>
                <Sidebar.MenuButton>
                  {#snippet child({ props })}
                    <a href={item.url} {...props}>
                      <item.icon />
                      <span>{item.title}</span>
                    </a>
                  {/snippet}
                </Sidebar.MenuButton>
              </Sidebar.MenuItem>
            {/each}
          </Sidebar.Menu>
        </Sidebar.GroupContent>
      </Sidebar.Group>
    </Sidebar.Content>
  </Sidebar.Root>