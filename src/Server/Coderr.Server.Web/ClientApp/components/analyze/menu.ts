import * as MenuApi from "../../services/menu/MenuApi";
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { MyIncidents, IMyIncident } from "./myincidents";

interface IRouteNavigation {
    routeName: string;
    url: string;
    setMenu(name: String): void;
}

type NavigationCallback = (context: IRouteNavigation) => void;

@Component
export default class AnalyzeMenuComponent extends Vue {
    childMenu: MenuApi.MenuItem[] = [];

    incidents: IMyIncident[] = [];
    title = '';
    incidentId: number | null = null;

    created() {
        MyIncidents.Instance.subscribeOnSelectedIncident(this.onIncidentSelected);
        MyIncidents.Instance.subscribeOnListChanges(this.onListChanged);
    }

    mounted() {
        MyIncidents.Instance.ready()
            .then(() => {
                if (MyIncidents.Instance.incident) {
                    this.incidentId = MyIncidents.Instance.incident.incidentId;
                    this.title = MyIncidents.Instance.incident.title;
                }
                this.incidents = MyIncidents.Instance.myIncidents;
            });
    }

    destroyed() {
        MyIncidents.Instance.unsubscribe(this.onIncidentSelected);
        MyIncidents.Instance.unsubscribe(this.onListChanged);
    }

    private onListChanged() {
        this.incidents = MyIncidents.Instance.myIncidents;
        if (!this.incidentId && this.incidents.length > 0) {
            this.incidentId = this.incidents[0].incidentId;
            MyIncidents.Instance.switchIncident(this.incidentId);
        }
    }

    private onIncidentSelected(incident: IMyIncident | null) {
        if (incident == null) {
            this.title = '(Select an incident)';
            this.incidentId = null;
        } else {
            this.$router.push({ name: 'analyzeIncident', params: { incidentId: incident.incidentId.toString() } });
            this.title = incident.shortTitle;
            this.incidentId = incident.incidentId;
        }
    }

    @Watch('$route.params.incidentId')
    onIncidentRoute(value: string, oldValue: string) {
        if (this.$route.fullPath.indexOf('/analyze/') === -1) {
            return;
        }
        if (!value) {
            this.incidentId = null;
            return;
        } else {
            var newIncidentId = parseInt(value, 10);
            if (this.incidentId === newIncidentId) {
                return;
            }
            this.incidentId = newIncidentId;
        }

        MyIncidents.Instance.switchIncident(this.incidentId);
    }

}