import { ContentBox } from "../components/ContentBox";
import { Layout } from "../components/Layout";
import SliderCollection from "../components/SliderCollection";

export default function LiveViewPage() {
  return (
    <Layout title="Live View">
      <ContentBox>
        <SliderCollection />
      </ContentBox>
    </Layout>
  );
}
